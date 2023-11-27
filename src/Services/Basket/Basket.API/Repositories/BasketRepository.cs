using System.Text.Json;
using System.Text.Json.Serialization;
using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);
            if (string.IsNullOrEmpty(basket))
                return null;

            return JsonSerializer.Deserialize<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            var json = JsonSerializer.Serialize(basket);
            await _redisCache.SetStringAsync(basket.UserName, json);

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
        }
    }
}
