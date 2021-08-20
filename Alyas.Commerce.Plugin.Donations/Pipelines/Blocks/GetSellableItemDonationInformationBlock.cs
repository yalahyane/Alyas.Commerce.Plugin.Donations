namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    public class GetSellableItemDonationInformationBlock : PipelineBlock<IEnumerable<string>, IEnumerable<DonationInformation>, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commander;
        public GetSellableItemDonationInformationBlock(CommerceCommander commander)
        {
            this._commander = commander;
        }

        public override async Task<IEnumerable<DonationInformation>> Run(IEnumerable<string> arg, CommercePipelineExecutionContext context)
        {
            var list = arg.ToList();
            Condition.Requires(list).IsNotNull($"{this.Name}: argument cannot be null.");
            if (!list.Any())
                return Enumerable.Empty<DonationInformation>();

            var items = new List<DonationInformation>();
            foreach (var itemId in list)
            {
                var productArgument = ProductArgument.FromItemId(itemId);
                if (!productArgument.IsValid())
                {
                    await context.CommerceContext.AddMessage(context.CommerceContext.GetPolicy<KnownResultCodes>().Error, "ItemIdIncorrectFormat", new object[]
                    {
                        itemId
                    }, $"Expecting a CatalogId and a ProductId in the ItemId: {itemId}.");
                }
                else
                {
                    var sellableItem = context.CommerceContext.GetEntity<SellableItem>(s => s.ProductId.Equals(productArgument.ProductId, StringComparison.OrdinalIgnoreCase));
                    if (sellableItem == null)
                    {
                        sellableItem = await this._commander.Pipeline<IGetSellableItemPipeline>().Run(productArgument, context.CommerceContext.PipelineContextOptions);
                    }
                    if (sellableItem != null)
                    {
                        var donationInformationPolicy = sellableItem.GetPolicy<DonationInformationPolicy>();
                        var currentDonation = donationInformationPolicy.Donations.FirstOrDefault(x => x.CurrencyCode.Equals(context.CommerceContext.CurrentCurrency(), StringComparison.OrdinalIgnoreCase));
                        if (currentDonation != null)
                        {
                            currentDonation.ItemId = sellableItem.FriendlyId;
                            items.Add(currentDonation);
                        }
                    }
                }
            }

            return items;
        }
    }
}
