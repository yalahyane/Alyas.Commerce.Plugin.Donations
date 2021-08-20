namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    public class DoActionRemoveDonationInformationBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commander;
        public DoActionRemoveDonationInformationBlock(CommerceCommander commander)
        {
            this._commander = commander;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            if (string.IsNullOrEmpty(entityView.Action) || !entityView.Action.Equals("RemoveDonationInformation", StringComparison.OrdinalIgnoreCase))
                return entityView;
            var entity = await this._commander.Pipeline<IFindEntityPipeline>().Run(new FindEntityArgument(typeof(SellableItem), entityView.EntityId), context);
            if (!(entity is SellableItem))
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "EntityNotFound", new object[]
                {
                    entityView.EntityId
                }, $"Entity { entityView.EntityId} was not found.");
                return entityView;
            }
            var sellableItem = entity as SellableItem;
            var donationId = entityView.ItemId.Split('|')[0];
            var currency = entityView.ItemId.Split('|')[1];
            var donationInformationPolicy = sellableItem.GetPolicy<DonationInformationPolicy>();
            var existingDonationInformation = donationInformationPolicy.Donations.FirstOrDefault(x => x.DonationId.Equals(donationId, StringComparison.OrdinalIgnoreCase) && 
                                                                                             x.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase));
            if (existingDonationInformation == null)
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "DonationInformationNotFound", new object[]
                    {
                        currency,
                        $"{ sellableItem.Id}"
                    },
                    $"Donation Information for '{ currency}' was not found in '{ sellableItem.Id}'");
                return entityView;
            }
            donationInformationPolicy.RemoveDonation(existingDonationInformation);

            await this._commander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);


            return entityView;
        }
    }
}
