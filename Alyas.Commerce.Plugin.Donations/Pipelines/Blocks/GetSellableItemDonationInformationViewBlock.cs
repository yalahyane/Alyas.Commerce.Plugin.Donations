namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;

    public class GetSellableItemDonationInformationViewBlock : GetListViewBlock
    {
        public GetSellableItemDonationInformationViewBlock(CommerceCommander commerceCommander) : base(commerceCommander)
        {
        }

        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(request?.ViewName) || !request.ViewName.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase) && (!request.ViewName.Equals(viewsPolicy.Variant, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(viewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(entityView);

            if (!(request.Entity is SellableItem item) || !string.IsNullOrEmpty(request.ForAction))
                return Task.FromResult(entityView);

            var isAddAction = request.ForAction.Equals("AddSellableItem", StringComparison.OrdinalIgnoreCase);

            if (isAddAction)
                return Task.FromResult(entityView);

            this.AddSellableItemDonations(entityView, item, context);

            return Task.FromResult(entityView);
        }

        protected virtual void AddSellableItemDonations(EntityView entityView, SellableItem entity, CommercePipelineExecutionContext context)
        {
            var donationInformationView = new EntityView()
            {
                Name = "DonationInformation",
                DisplayName = "Donation Information",
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = string.Empty,
                UiHint = "Table"
            };

            foreach (var donation in entity.GetPolicy<DonationInformationPolicy>().Donations)
            {
                var summaryView = new EntityView
                {
                    Name = context.GetPolicy<KnownCatalogViewsPolicy>().Summary,
                    EntityId = entityView.EntityId,
                    ItemId = $"{donation.DonationId}|{donation.CurrencyCode}",
                    UiHint = "Flat"
                };
                var currencyProperty = new ViewProperty
                {
                    Name = "Currency",
                    RawValue = donation.CurrencyCode
                };
                summaryView.Properties.Add(currencyProperty);
                var donationIdProperty = new ViewProperty
                {
                    Name = "DonationId",
                    RawValue = donation.DonationId,
                    DisplayName = "Donation Id"
                };
                summaryView.Properties.Add(donationIdProperty);
                var minDonationProperty = new ViewProperty
                {
                    Name = "MinimumDonation",
                    RawValue = donation.MinimumDonation,
                    DisplayName = "Minimum Donation"
                };
                summaryView.Properties.Add(minDonationProperty);
                var donationOptionsProperty = new ViewProperty
                {
                    Name = "DonationOptions",
                    RawValue = donation.DonationOptions,
                    DisplayName = "Donation Options"
                };
                summaryView.Properties.Add(donationOptionsProperty);

                
                donationInformationView.ChildViews.Add(summaryView);
            }
            entityView.ChildViews.Add(donationInformationView);
        }
    }
}
