namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    public class DoActionAddEditDonationInformationBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commander;
        public DoActionAddEditDonationInformationBlock(CommerceCommander commander)
        {
            this._commander = commander;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            var isAddAction = entityView.Action != null && entityView.Action.Equals("AddDonationInformation", StringComparison.OrdinalIgnoreCase);
            var isEditAction = entityView.Action != null && entityView.Action.Equals("EditDonationInformation", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(entityView.Action) || !isAddAction && !isEditAction)
            {
                return entityView;
            }

            var currencyProperty = entityView.Properties.FirstOrDefault(p => p.Name.Equals("Currency", StringComparison.OrdinalIgnoreCase));
            var currency = currencyProperty?.Value;
            if (string.IsNullOrWhiteSpace(currency))
            {
                var displayName = currencyProperty == null ? "Currency" : currencyProperty.DisplayName;
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingPropertyValue", new object[]
                {
                    displayName
                }, "Invalid or missing value for property 'Currency'.");
                return entityView;
            }

            var donationIdProperty = entityView.Properties.FirstOrDefault(p => p.Name.Equals("DonationId", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(donationIdProperty?.Value))
            {
                var displayName = donationIdProperty == null ? "Donation Id" : donationIdProperty.DisplayName;
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingPropertyValue", new object[]
                {
                    displayName
                }, "Invalid or missing value for property 'DonationId'.");
                return entityView;
            }

            var minDonationProperty = entityView.Properties.FirstOrDefault(p => p.Name.Equals("MinimumDonation", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(minDonationProperty?.Value) || !decimal.TryParse(minDonationProperty.Value, out var minDonation))
            {
                var displayName = minDonationProperty == null ? "Minimum Donation" : minDonationProperty.DisplayName;
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingPropertyValue", new object[]
                {
                    displayName
                }, "Invalid or missing value for property 'MinDonation'.");
                return entityView;
            }

            var donationOptionsProperty = entityView.Properties.FirstOrDefault(p => p.Name.Equals("DonationOptions", StringComparison.OrdinalIgnoreCase));
            var donationOptions = donationOptionsProperty?.Value;

            var entity = await this._commander.Pipeline<IFindEntityPipeline>().Run(new FindEntityArgument(typeof(SellableItem), entityView.EntityId), context);
            if (!(entity is SellableItem))
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "EntityNotFound", new object[]
                {
                    entityView.EntityId
                }, $"Entity {entityView.EntityId} was not found.");
                return entityView;
            }
            var sellableItem = entity as SellableItem;
            var donationInformationPolicy = sellableItem.GetPolicy<DonationInformationPolicy>();
            var existingDonationInformation = donationInformationPolicy.Donations.FirstOrDefault(x => x.DonationId.Equals(donationIdProperty.Value, StringComparison.OrdinalIgnoreCase) &&
                                                                                             x.CurrencyCode.Equals(currency, StringComparison.OrdinalIgnoreCase));
            if (isAddAction)
            {
                if (existingDonationInformation != null)
                {
                    await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "DonationInformationAlreadyExists", new object[]
                    {
                        donationIdProperty.Value,
                        currency,
                        $"{sellableItem.Id}"
                    }, $"Donation Information for Donation Id  '{donationIdProperty.Value}', Currency '{currency}' already exists in '{sellableItem.Id}'");
                    return entityView;
                }

                donationInformationPolicy.AddDonation(new DonationInformation
                {
                    ItemId = sellableItem.FriendlyId,
                    CurrencyCode = currency,
                    DonationId = donationIdProperty.Value,
                    MinimumDonation = minDonation,
                    DonationOptions = donationOptions

                });
            }
            else
            {
                if (existingDonationInformation == null)
                {
                    await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "DonationInformationNotFound", new object[]
                    {
                        currency,
                        $"{sellableItem.Id}"
                    }, $"Donation Information for '{currency}' was not found in '{sellableItem.Id}'");
                    return entityView;
                }

                existingDonationInformation.DonationId = donationIdProperty.Value;
                existingDonationInformation.MinimumDonation = minDonation;
                existingDonationInformation.DonationOptions = donationOptions;
            }

            await this._commander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);

            return entityView;
        }
    }
}
