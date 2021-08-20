namespace Alyas.Commerce.Plugin.Donations.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http.OData;
    using Commands;
    using Microsoft.AspNetCore.Mvc;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;

    [Route("api")]
    public class CommandsController : CommerceController
    {
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment) : base(
            serviceProvider, globalEnvironment)
        {
        }

        [HttpPost]
        [Route("AddDonationCartLine")]
        [EnableQuery]
        public async Task<IActionResult> AddDonationCartLine([FromBody] ODataActionParameters value)
        {
            var commandsController = this;
            if (!commandsController.ModelState.IsValid || value == null)
                return new BadRequestObjectResult(commandsController.ModelState);
            if (!value.ContainsKey("cartId") || string.IsNullOrEmpty(value["cartId"]?.ToString()) || !value.ContainsKey("itemId") || 
                string.IsNullOrEmpty(value["itemId"]?.ToString()) || !value.ContainsKey("quantity") || string.IsNullOrEmpty(value["quantity"]?.ToString()) || 
                !value.ContainsKey("donationAmount") || string.IsNullOrEmpty(value["donationAmount"]?.ToString()))
            {
                return new BadRequestObjectResult(value);
            }
            var cartId = value["cartId"].ToString();
            var itemId = value["itemId"].ToString();
            if (!decimal.TryParse(value["quantity"].ToString(), out var quantity))
            {
                return new BadRequestObjectResult(value);
            }

            if (!decimal.TryParse(value["donationAmount"].ToString(), out var donationAmount))
            {
                return new BadRequestObjectResult(value);
            }

            
            var command = commandsController.Command<AddDonationCartLineCommand>();
            await command.Process(
                commandsController.CurrentContext, donationAmount, cartId, new CartLineComponent
                {
                    ItemId = itemId,
                    Quantity = quantity
                }).ConfigureAwait(false);

            return new ObjectResult(command);
        }
    }
}
