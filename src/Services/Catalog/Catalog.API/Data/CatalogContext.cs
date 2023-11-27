using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(IConfiguration conf)
        {
            var client = new MongoClient(conf.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(conf.GetValue<string>("DatabaseSettings:DatabaseName"));

            Products = database.GetCollection<Product>(conf.GetValue<string>("DatabaseSettings:CollectionName"));
            CatalogContextSeed.SeedData(Products);
        }

        public IMongoCollection<Product> Products { get; }
    }
}
