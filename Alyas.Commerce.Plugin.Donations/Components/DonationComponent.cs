namespace Alyas.Commerce.Plugin.Donations.Components
{
    using Sitecore.Commerce.Core;

    public class DonationComponent : Component
    {
        public decimal DonationAmount { get; set; } = 0;
        public string DonationId { get; set; }
    }
}
