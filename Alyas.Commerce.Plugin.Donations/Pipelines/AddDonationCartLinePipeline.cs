namespace Alyas.Commerce.Plugin.Donations.Pipelines
{
    using Arguments;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Pipelines;

    public class AddDonationCartLinePipeline : CommercePipeline<AddDonationCartLineArgument, Cart>, IAddDonationCartLinePipeline
    {
        public AddDonationCartLinePipeline(IPipelineConfiguration<IAddDonationCartLinePipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
