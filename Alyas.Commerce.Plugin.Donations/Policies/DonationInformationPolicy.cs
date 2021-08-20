namespace Alyas.Commerce.Plugin.Donations.Policies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Sitecore.Commerce.Core;

    public class DonationInformationPolicy : Policy
    {
        private readonly List<DonationInformation> _donationInformations;
        public IEnumerable<DonationInformation> Donations  => this._donationInformations.AsEnumerable();

        public DonationInformationPolicy()
        {
            this._donationInformations = new List<DonationInformation>();
        }

        public virtual void AddDonation(DonationInformation donation)
        {
            if (string.IsNullOrEmpty(donation?.CurrencyCode) || string.IsNullOrEmpty(donation.DonationId))
                throw new InvalidOperationException("Donation cannot be null and must have a non-null DonationId|CurrencyCode.");
            var donationInformation = this._donationInformations.FirstOrDefault(p => p.DonationId.Equals(donation.DonationId, StringComparison.OrdinalIgnoreCase) && 
                                                                               p.CurrencyCode.Equals(donation.CurrencyCode, StringComparison.OrdinalIgnoreCase));
            if (donationInformation != null)
                this._donationInformations.Remove(donation);
            this._donationInformations.Add(donation);
        }

        public virtual void RemoveDonation(DonationInformation donation)
        {
            if (string.IsNullOrEmpty(donation?.CurrencyCode) || string.IsNullOrEmpty(donation.DonationId))
                throw new InvalidOperationException("Donation cannot be null and must have a non-null DonationId|CurrencyCode.");
            var donationInformation = this._donationInformations.FirstOrDefault(p => p.DonationId.Equals(donation.DonationId, StringComparison.OrdinalIgnoreCase) && 
                                                                               p.CurrencyCode.Equals(donation.CurrencyCode, StringComparison.OrdinalIgnoreCase));
            if (donationInformation == null)
                return;
            this._donationInformations.Remove(donation);
        }
    }
}
