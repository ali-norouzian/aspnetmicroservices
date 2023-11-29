using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.API.Extensions
{
    public static class MigrateAndSeedDbExtensions
    {
        public static async Task<WebApplication> MigrateAndSeedDb<TContext>(this WebApplication app, int retryCount = 0)
            where TContext : DbContext
        {
            // ReTry it 10 time
            if (10 < retryCount)
                throw new Exception("Database connection has problem!");

            await using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Migrating DB started...");

                var context = services.GetRequiredService<OrderContext>();
                //var userManager = services.GetRequiredService<UserManager<AppUser>>();

                await context.Database.MigrateAsync();
                await Seed(context);

                logger.LogInformation("Migrating DB finished...");
            }
            catch (Exception ex)
            {
                logger.LogError("Migrating DB failed. ReTry in 1 second ...");
                await Task.Delay(1000);
                await MigrateAndSeedDb<TContext>(app, retryCount + 1);
            }

            return app;
        }

        private static async Task Seed(OrderContext orderContext)
        {
            orderContext.Orders.AddRange(GetPreconfiguredOrders());
            await orderContext.SaveChangesAsync();
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() {UserName = "theesan", FirstName = "esan", LastName = "nz", EmailAddress = "theesanc@gmail.com", AddressLine = "somewhere", Country = "ir", TotalPrice = 350}
            };
        }
    }

}