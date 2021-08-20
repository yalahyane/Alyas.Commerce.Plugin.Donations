namespace Alyas.Commerce.Plugin.Donations.Pipelines
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Models;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Pipelines;

    public class GetSellableItemDonationInformationPipeline : CommercePipeline<IEnumerable<string>, IEnumerable<DonationInformation>>, IGetSellableItemDonationInformationPipeline
    {
        public GetSellableItemDonationInformationPipeline(IPipelineConfiguration<IGetSellableItemDonationInformationPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
