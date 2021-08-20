namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    public class GetSellableItemDonationInformationEditBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCurrencySetCommand _getCurrencySetCommand;

        public GetSellableItemDonationInformationEditBlock(GetCurrencySetCommand getCurrencySetCommand)
        {
            this._getCurrencySetCommand = getCurrencySetCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            var entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            var forAction = entityViewArgument.ForAction;

            var editAction = forAction.Equals("EditDonationInformation", StringComparison.OrdinalIgnoreCase);
            var addAction = forAction.Equals("AddDonationInformation", StringComparison.OrdinalIgnoreCase);

            if (!(addAction || editAction))
            {
                return entityView;
            }


            if (!(entityViewArgument.Entity is SellableItem sellableItem))
            {
                return entityView;
            }

            var policy = context.GetPolicy<GlobalEnvironmentPolicy>();
            var donationId = entityView.ItemId.Split('|')[0];
            var currencyCode = string.IsNullOrEmpty(donationId)? string.Empty : entityView.ItemId.Split('|')[1];

            var donationInformationPolicy = sellableItem.GetPolicy<DonationInformationPolicy>();
            var currencyProperty = new ViewProperty
            {
                Name = "Currency",
                RawValue = editAction ? currencyCode : policy.DefaultCurrency,
                IsReadOnly = editAction,
                IsRequired = addAction,
                IsHidden = false
            };
            if (addAction)
            {
                var currencySet = await this._getCurrencySetCommand.Process(context.CommerceContext, context.GetPolicy<GlobalCurrencyPolicy>().DefaultCurrencySet);
                currencyProperty.Policies = new List<Policy>()
            {
              new AvailableSelectionsPolicy()
              {
                  List = (currencySet.HasComponent<CurrenciesComponent>() ? currencySet.GetComponent<CurrenciesComponent>().Currencies.Select(c => new Selection
                  {
                      DisplayName = c.Code,
                      Name = c.Code,
                      IsDefault = c.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase)
                  }).ToList() : new List<Selection>())
              }
            };
            }
            entityView.Properties.Add(currencyProperty);

            var donation = donationInformationPolicy.Donations.FirstOrDefault(x => x.DonationId.Equals(donationId, StringComparison.OrdinalIgnoreCase) && 
                                                                          x.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

            var donationIdProperty = new ViewProperty
            {
                Name = "DonationId",
                RawValue = !editAction || donation == null ? string.Empty : donation.DonationId,
                IsReadOnly = false,
                IsRequired = true,
                IsHidden = false
            };
            entityView.Properties.Add(donationIdProperty);

            var minDonationProperty = new ViewProperty
            {
                Name = "MinDonation",
                RawValue = !editAction || donation == null ? decimal.Zero : donation.MinimumDonation,
                IsReadOnly = false,
                IsRequired = true,
                IsHidden = false
            };
            entityView.Properties.Add(minDonationProperty);

            var donationOptionsProperty = new ViewProperty
            {
                Name = "DonationOptions",
                RawValue = !editAction || donation == null ? string.Empty: donation.DonationOptions,
                IsReadOnly = false,
                IsRequired = false,
                IsHidden = false
            };
            entityView.Properties.Add(donationOptionsProperty);

            return entityView;
        }
    }
}
