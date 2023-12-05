using Basket.API.Entities;
using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcServices
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

        public DiscountGrpcServices(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        {
            _discountProtoService = discountProtoService;
        }

        public async Task<ShoppingCart> GetDiscount(ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var discountRequest = new GetDiscountRequest { ProductName = item.ProductName };
                var coupon = await _discountProtoService.GetDiscountAsync(discountRequest);
                item.Price -= coupon.Amount;
            }

            return basket;
        }
    }
}
