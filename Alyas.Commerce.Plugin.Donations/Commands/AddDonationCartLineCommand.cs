namespace Alyas.Commerce.Plugin.Donations.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Pipelines;
    using Pipelines.Arguments;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.ManagedLists;

    public class AddDonationCartLineCommand : CommerceCommand
    {
        private readonly IAddDonationCartLinePipeline _addDonationCartLinePipeline;
        private readonly IFindEntityPipeline _getPipeline;

        public AddDonationCartLineCommand(IFindEntityPipeline getCartPipeline, IAddDonationCartLinePipeline addDonationCartLinePipeline, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._addDonationCartLinePipeline = addDonationCartLinePipeline;
            this._getPipeline = getCartPipeline;
        }

        public virtual async Task<Cart> Process(CommerceContext commerceContext, decimal donation, string cartId, CartLineComponent line)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                var context = commerceContext.PipelineContextOptions;
                var findEntityArgument = new FindEntityArgument(typeof(Cart), cartId, true);
                if (await this._getPipeline.Run(findEntityArgument, context) is Cart cart)
                {
                    if (!cart.IsPersisted)
                    {
                        cart.Id = cartId;
                        cart.Name = cartId;
                        cart.ShopName = commerceContext.CurrentShopName();
                        cart.SetComponent(new ListMembershipsComponent()
                        {
                            Memberships = new List<string>()
                            {
                                CommerceEntity.ListName<Cart>()
                            }
                        });
                    }

                    var arg = new AddDonationCartLineArgument(donation, cart, line);
                    var result = await this._addDonationCartLinePipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));

                    return result;
                }
                await context.CommerceContext.AddMessage(commerceContext.GetPolicy<KnownResultCodes>().ValidationError, "EntityNotFound", new object[]
                {
                    cartId
                }, $"Entity {cartId} was not found.");
                return null;
            }
        }
    }
}