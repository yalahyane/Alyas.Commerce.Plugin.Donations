namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Pipelines;

    public class PopulateSellableItemsDonationInformationEditActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            var entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            if (!(entityViewArgument?.Entity is SellableItem) || !string.IsNullOrEmpty(entityViewArgument.ForAction))
                return Task.FromResult(entityView);
            var entity = (SellableItem)entityViewArgument.Entity;
            var name = entityView.Name;
            var actionsPolicy = entityView.GetPolicy<ActionsPolicy>();

            if (name.Equals("DonationInformation", StringComparison.OrdinalIgnoreCase))
            {
                var donationInformationPolicy = entity.GetPolicy<DonationInformationPolicy>(entityView.ItemId);
                var actions = actionsPolicy.Actions;
                var addActionView = new EntityActionView
                {
                    Name = "AddDonationInformation",
                    DisplayName = "Add Donation Information",
                    Description = "Add Donation Information",
                    IsEnabled = true,
                    EntityView = name,
                    RequiresConfirmation = false,
                    Icon = "add"
                };
                actions.Add(addActionView);
                var editActionView = new EntityActionView
                {
                    Name = "EditDonationInformation",
                    DisplayName = "Edit Donation Information",
                    Description = "Edits Donation Information",
                    IsEnabled = donationInformationPolicy.Donations.Any(),
                    EntityView = name,
                    RequiresConfirmation = false,
                    Icon = "edit"
                };
                actions.Add(editActionView);
                var entityActionView3 = new EntityActionView
                {
                    Name = "EditDonationInformation",
                    DisplayName = "Remove Donation Information",
                    Description = "Removes Donation Information",
                    IsEnabled = donationInformationPolicy.Donations.Any(),
                    EntityView = string.Empty,
                    RequiresConfirmation = true,
                    Icon = "delete"
                };
                actions.Add(entityActionView3);
            }

            return Task.FromResult(entityView);
        }
    }
}
