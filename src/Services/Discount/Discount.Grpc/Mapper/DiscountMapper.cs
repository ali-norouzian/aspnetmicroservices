using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;

namespace Discount.Grpc.Mapper
{
    public class DiscountMapper : Profile
    {
        public DiscountMapper()
        {
            CreateMap<Coupon, CouponModel>().ReverseMap();
        }
    }
}
