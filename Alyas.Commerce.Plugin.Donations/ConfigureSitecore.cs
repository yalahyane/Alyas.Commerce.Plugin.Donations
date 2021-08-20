namespace Alyas.Commerce.Plugin.Donations
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;
    using Pipelines.Blocks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.DigitalItems;
    using Sitecore.Commerce.Plugin.GiftCards;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IGetEntityViewPipeline>(configure =>
                {
                    configure.Add<GetSellableItemDonationInformationViewBlock>().After<GetSellableItemDetailsViewBlock>();
                    configure.Add<GetSellableItemDonationInformationEditBlock>().After<GetSellableItemEditBlock>();
                })
                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(configure =>
                {
                    configure.Add<PopulateSellableItemsDonationInformationEditActionsBlock>().After<PopulateSellableItemsEditActionsBlock>();
                })
                .ConfigurePipeline<IDoActionPipeline>(configure =>
                {
                    configure.Add<DoActionAddEditDonationInformationBlock>().After<DoActionAddEditListPriceBlock>();
                    configure.Add<DoActionRemoveDonationInformationBlock>().After<DoActionRemoveListPriceBlock>();
                })
                .AddPipeline<IGetSellableItemDonationInformationPipeline, GetSellableItemDonationInformationPipeline>(
                    configure =>
                    {
                        configure.Add<IgnoreAvailabilityBlock<IEnumerable<string>>>();
                        configure.Add<GetSellableItemDonationInformationBlock>();
                    })
                .AddPipeline<IAddDonationCartLinePipeline, AddDonationCartLinePipeline>(
                    configure =>
                    {
                        configure
                            .Add<ValidateSellableItemBlock>()
                            .Add<AddDonationCartLineValidationBlock>()
                            .Add<AddDonationCartLineBlock>()
                            .Add<AddContactBlock>()
                            .Add<IPopulateValidateCartPipeline>()
                            .Add<AddCartLineGiftCardBlock>()
                            .Add<AddCartLineDigitalProductBlock>()
                            .Add<AddCartLineWarrantyBlock>()
                            .Add<AddCartLineInstallationBlock>()
                            .Add<ICalculateCartLinesPipeline>()
                            .Add<ICalculateCartPipeline>()
                            .Add<PrepArgumentToPersistEntityBlock<Cart>>().Add<IPersistEntityPipeline>().Add<PostPersistEntityBlock<Cart>>();
                    })
                .ConfigurePipeline<ICalculateCartLinesPipeline>(configure => configure.Add<CalculateDonationCartLinesSubTotalsBlock>().After<CalculateCartLinesSubTotalsBlock>(), "main", 50000)
               );
            
            services.RegisterAllCommands(assembly);
        }
    }
}