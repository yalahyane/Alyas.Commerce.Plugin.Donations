namespace Alyas.Commerce.Plugin.Donations.Pipelines
{
    using System.Collections.Generic;
    using Models;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Pipelines;

    public interface IGetSellableItemDonationInformationPipeline : IPipeline<IEnumerable<string>, IEnumerable<DonationInformation>, CommercePipelineExecutionContext>
    {
    }
}
