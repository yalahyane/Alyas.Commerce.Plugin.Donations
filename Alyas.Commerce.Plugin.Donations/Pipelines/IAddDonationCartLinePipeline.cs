namespace Alyas.Commerce.Plugin.Donations.Pipelines
{
    using Arguments;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Pipelines;

    public interface IAddDonationCartLinePipeline : IPipeline<AddDonationCartLineArgument, Cart, CommercePipelineExecutionContext>
    {
    }
}
