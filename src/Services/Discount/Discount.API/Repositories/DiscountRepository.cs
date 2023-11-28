using Dapper;
using Discount.API.Entities;
using Npgsql;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private const string CouponTableName = "Coupon";
        private readonly IConfiguration _configuration;
        private readonly string ConnectionString;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            ConnectionString = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);

            var query = @$"SELECT * 
                        FROM {CouponTableName} 
                        WHERE ProductName = @ProductName";
            var parameters = new { ProductName = productName };

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(query, parameters);
            if (coupon == null)
                return new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };

            return coupon;
            // The connection is automatically disposed at the end of the using block
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);

            var query = @$"INSERT INTO {CouponTableName} 
                        (ProductName, Description, Amount)
                        VALUES (@ProductName, @Description, @Amount)";
            var parameters = coupon;

            var affected = await connection.ExecuteAsync(query, parameters);
            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);

            var query = @$"UPDATE {CouponTableName} 
                        SET ProductName=@ProductName,
                            Description=@Description, 
                            Amount=@Amount
                        WHERE Id = @Id ";
            var parameters = coupon;

            var affected = await connection.ExecuteAsync(query, parameters);
            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);

            var query = @$"DELETE FROM {CouponTableName} 
                        WHERE ProductName=@ProductName ";
            var parameters = new { ProductName = productName };

            var affected = await connection.ExecuteAsync(query, parameters);
            if (affected == 0)
                return false;

            return true;
        }
    }
}
