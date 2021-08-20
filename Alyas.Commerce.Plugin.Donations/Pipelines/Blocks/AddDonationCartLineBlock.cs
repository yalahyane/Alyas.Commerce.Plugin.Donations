namespace Alyas.Commerce.Plugin.Donations.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Arguments;
    using Components;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("Cart.AddDonationCartLineBlock")]
    public class AddDonationCartLineBlock : PipelineBlock<AddDonationCartLineArgument, Cart, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public AddDonationCartLineBlock(CommerceCommander commerceCommander)
        {
            this._commerceCommander = commerceCommander;
        }

        public override async Task<Cart> Run(AddDonationCartLineArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            Condition.Requires(arg.Cart).IsNotNull($"{this.Name}: The cart can not be null");
            Condition.Requires(arg.Line).IsNotNull($"{this.Name}: The line to add can not be null");
            context.CommerceContext.AddObject(arg);

            var error = context.GetPolicy<KnownResultCodes>().Error;

            var cart = arg.Cart;
            var lineQuantityPolicy = context.GetPolicy<LineQuantityPolicy>();
            var quantity = arg.Line.Quantity;
            var existingLine = cart.Lines.FirstOrDefault(line => line.Id.Equals(arg.Line.Id, StringComparison.OrdinalIgnoreCase));

            var flag1 = !await lineQuantityPolicy.IsValid(quantity, context.CommerceContext);
            if (!flag1)
            {
                var flag2 = existingLine != null;
                if (flag2)
                    flag2 = !await lineQuantityPolicy.IsValid(quantity + existingLine.Quantity, context.CommerceContext);
                flag1 = flag2;
            }
            if (flag1)
            {
                context.Abort("Invalid or missing value for property 'Quantity'.", context);
                return cart;
            }


            if (!string.IsNullOrEmpty(arg.Line.ItemId))
            {
                if (arg.Line.ItemId.Split('|').Length >= 3)
                {
                    if (string.IsNullOrEmpty(arg.Line.Id))
                        arg.Line.Id = Guid.NewGuid().ToString("N");
                    var list = cart.Lines.ToList();
                    if (!context.CommerceContext.GetPolicy<RollupCartLinesPolicy>().Rollup)
                    {
                        list.Add(arg.Line);
                        arg.Line.UnitListPrice = new Money(arg.DonationAmount);
                        context.CommerceContext.AddModel(new LineAdded(arg.Line.Id));
                    }
                    else if (existingLine != null)
                    {
                        existingLine.Quantity += arg.Line.Quantity;
                        arg.Line.UnitListPrice = new Money(arg.DonationAmount);
                        arg.Line.Id = existingLine.Id;
                        context.CommerceContext.AddModel(new LineUpdated(arg.Line.Id));
                    }
                    else
                    {
                        list.Add(arg.Line);

                        var donationInformationList = await this._commerceCommander.Pipeline<IGetSellableItemDonationInformationPipeline>().Run(new List<string> { arg.Line.ItemId }, context);
                        var donationInformation = donationInformationList?.FirstOrDefault();
                        if (donationInformation == null)
                        {
                            context.Abort(await context.CommerceContext.AddMessage(error, "AddDonationMissingDonationInformation", new object[] { arg.Line.ItemId }, "DonationInformation is missing"), context);
                            return cart;
                        }

                        var donationId = donationInformation.DonationId;

                        arg.Line.SetComponent(new DonationComponent()
                        {
                            DonationAmount = arg.DonationAmount,
                            DonationId = donationId
                        });
                        context.CommerceContext.AddModel(new LineAdded(arg.Line.Id));
                    }
                    cart.Lines = list;
                    return cart;
                }
            }
            var message = $"Expecting a CatalogId and a ProductId in the ItemId: {arg.Line.ItemId}.";
            context.Abort(await context.CommerceContext.AddMessage(error, "ItemIdIncorrectFormat", new object[] { arg.Line.ItemId }, message), context);
            return cart;
        }
    }
}
