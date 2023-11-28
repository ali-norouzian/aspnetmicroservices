using Dapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Repositories;
using Npgsql;

namespace Discount.Grpc.Data
{
    public static class SeedData
    {
        public static async Task<WebApplication> SeedCouponData(this WebApplication app, WebApplicationBuilder builder, int retryCount = 0)
        {
            // ReTry it 10 time
            if (10 < retryCount)
                throw new Exception("Database connection has problem!");

            var serviceProvider = builder.Services.BuildServiceProvider();
            await using var scope = serviceProvider.CreateAsyncScope();
            var _discountRepository = scope.ServiceProvider.GetService<IDiscountRepository>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var _connectionString = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var _logger = loggerFactory.CreateLogger("SeedDataLogger");


            await using var connection = new NpgsqlConnection(_connectionString);

            var data = new List<Coupon>()
            {
                new Coupon()
                {
                    ProductName = "Samsung 10",
                    Description = "Samsung Discount",
                    Amount =100 ,
                },
                new Coupon()
                {
                    ProductName = "IPhone X",
                    Description = "IPhone Discount",
                    Amount =150 ,
                }
            };

            try
            {
                _logger.LogInformation("Migrating DB started...");

                var query = @"SELECT COUNT(*) 
                              FROM Coupon;";
                var couponCount = await connection.ExecuteScalarAsync<int>(query);
                if (couponCount == 0)
                    foreach (var coupon in data)
                        await _discountRepository.CreateDiscount(coupon);

                _logger.LogInformation("Migrating DB finished...");
            }
            catch (NpgsqlException e)
            {
                try
                {
                    var query = @"CREATE TABLE Coupon(
                      ID SERIAL PRIMARY KEY       NOT NULL,
                      ProductName     VARCHAR(24) NOT NULL,
                      Description     TEXT,
                      Amount          INT); ";
                    await connection.ExecuteAsync(query);

                    // ReTry it 10 time
                    _logger.LogError("Migrating DB failed. ReTry in 1 second ...");
                    await Task.Delay(1000);
                    await app.SeedCouponData(builder, retryCount + 1);
                }
                catch (NpgsqlException ex)
                {
                    // ReTry it 10 time
                    _logger.LogError("Migrating DB failed. ReTry in 1 second ...");
                    await Task.Delay(1000);
                    await app.SeedCouponData(builder, retryCount + 1);
                }
            }

            return app;
        }
    }
}
