namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Components;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("Carts.CalculateDonationCartLinesSubTotalsBlock")]
    public class CalculateDonationCartLinesSubTotalsBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The cart can not be null");
            if (!arg.Lines.Any()) Task.FromResult(arg);
            var currencyCode = context.CommerceContext.CurrentCurrency();
            var lines = arg.Lines;

            if (lines == null || !lines.Any()) return Task.FromResult(arg);

            foreach (var line in lines.Where(l => l != null))
            {
                var donationComponent = line.GetComponent<DonationComponent>();
                if (donationComponent == null || donationComponent.DonationAmount <= decimal.Zero) continue;
                var price = donationComponent.DonationAmount * line.Quantity;
                line.Totals.SubTotal = new Money(currencyCode, price);
                line.Totals.GrandTotal = new Money(currencyCode, price);
                line.UnitListPrice = new Money(currencyCode, price);
            }
            return Task.FromResult(arg);
        }
    }
}
