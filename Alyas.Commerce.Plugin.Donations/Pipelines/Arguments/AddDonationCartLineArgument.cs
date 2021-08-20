namespace Alyas.Commerce.Plugin.Donations.Pipelines.Arguments
{
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;

    public class AddDonationCartLineArgument : CartLineArgument
    {
        public AddDonationCartLineArgument(decimal donationAmount, Cart cart, CartLineComponent line) : base(cart, line)
        {
            Condition.Requires(donationAmount).IsNotNull("The donation amount can not be null");

            this.DonationAmount = donationAmount;
        }

        public decimal DonationAmount { get; set; }
    }
}
