using Basket.API.Entities;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using Basket.API.GrpcServices;
using EventBus.Messages.Events;
using MassTransit;

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly DiscountGrpcServices _discountGrpcServices;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public BasketController(IBasketRepository basketRepository, DiscountGrpcServices discountGrpcServices, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _basketRepository = basketRepository;
            _discountGrpcServices = discountGrpcServices;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _basketRepository.GetBasket(userName);

            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            // Communicate with Dicount.Grpc
            // and Calculate latest prices of product into shopping cart.
            // Consume Discount Grpc: and reduce price with coupon
            await _discountGrpcServices.GetDiscount(basket);

            return Ok(await _basketRepository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> DeleteBasket(string userName)
        {
            await _basketRepository.DeleteBasket(userName);

            return Ok();
        }

        [HttpPost("[action]")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<ShoppingCart>> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // Get existing basket with total price
            // Create basketCheckoutEvent & Set TotalPrice on basketCheckout eventMessage
            var basket = await _basketRepository.GetBasket(basketCheckout.UserName);
            if (basket == null) return BadRequest();

            // Send checkout event to rabbitmq
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;
            // _eventBus.PublishBasketCheckout
            await _publishEndpoint.Publish(eventMessage);

            // Remove from queue
            await _basketRepository.DeleteBasket(basket.UserName);

            return Accepted();
        }
    }
}
