namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Arguments;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    public class AddDonationCartLineValidationBlock : PipelineBlock<AddDonationCartLineArgument, AddDonationCartLineArgument, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public AddDonationCartLineValidationBlock(CommerceCommander commerceCommander)
        {
            this._commerceCommander = commerceCommander;
        }

        public override async Task<AddDonationCartLineArgument> Run(AddDonationCartLineArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            Condition.Requires(arg.Cart).IsNotNull($"{this.Name}: The cart can not be null");
            Condition.Requires(arg.Line).IsNotNull($"{this.Name}: The line to add can not be null");
            var error = context.GetPolicy<KnownResultCodes>().Error;
            var productArgument = ProductArgument.FromItemId(arg.Line.ItemId);
            
            var donation = context.CommerceContext.GetEntity<SellableItem>(x =>
                            x.ProductId.Equals(productArgument.ProductId, StringComparison.OrdinalIgnoreCase)) ?? await this._commerceCommander.Pipeline<IFindEntityPipeline>()
                            .Run(new FindEntityArgument(typeof(SellableItem), productArgument.ProductId), context) as SellableItem;

            if (donation == null)
            {
                context.Abort(await context.CommerceContext.AddMessage(error, "AddDonationMissingDonationSellableItem", new object[] { arg.Line.ItemId }, "Donation SellableItem is missing"), context);
                return await Task.FromResult(arg);
            }

            var donationInformationList = await this._commerceCommander.Pipeline<IGetSellableItemDonationInformationPipeline>().Run(new List<string> {arg.Line.ItemId}, context);
            var donationInformation = donationInformationList?.FirstOrDefault();

            if (donationInformation == null)
            {
                context.Abort(await context.CommerceContext.AddMessage(error, "AddDonationMissingDonationInformation", new object[] { arg.Line.ItemId }, "DonationInformation is missing"), context);
                return await Task.FromResult(arg);
            }

            
            var minDonation = donationInformation.MinimumDonation;
                

            if (arg.DonationAmount >= minDonation)
                return await Task.FromResult(arg);

            var message = $"Expecting Donation to be >= {minDonation}.";
            context.Abort(await context.CommerceContext.AddMessage(error, "AddDonationIncorrectRange", new object[] { arg.Line.ItemId }, message), context);
            return await Task.FromResult(arg);
        }
    }
}
