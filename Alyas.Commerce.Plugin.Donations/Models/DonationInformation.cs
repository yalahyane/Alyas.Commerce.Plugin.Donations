namespace Alyas.Commerce.Plugin.Donations.Models
{
    using Sitecore.Commerce.Core;

    public class DonationInformation : Model
    {
        public string ItemId { get; set; }
        public string CurrencyCode { get; set; }
        public string DonationId { get; set; }
        public decimal MinimumDonation { get; set; }
        public string DonationOptions { get; set; }
    }
}
