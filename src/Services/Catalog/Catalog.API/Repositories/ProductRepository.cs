using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext _catalogContext;

        public ProductRepository(ICatalogContext catalogContext)
        {
            _catalogContext = catalogContext;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _catalogContext.Products
                .Find(p => true)
                .ToListAsync();
        }

        public async Task<Product> GetProduct(string id)
        {
            return await _catalogContext.Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByName(string name)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter
                .Eq(p => p.Name, name);

            return await _catalogContext.Products
                .Find(filter)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(string category)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter
                .Eq(p => p.Category, category);

            return await _catalogContext.Products
                .Find(filter)
                .ToListAsync();
        }

        public async Task CreateProduct(Product product)
        {
            await _catalogContext.Products.InsertOneAsync(product);
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var updateResult = await _catalogContext.Products
                .ReplaceOneAsync(filter: p => p.Id == product.Id, replacement: product);

            return updateResult.IsAcknowledged
                && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter
                .Eq(p => p.Id, id);

            var updateResult = await _catalogContext.Products
                .DeleteOneAsync(filter);

            return updateResult.IsAcknowledged
                && updateResult.DeletedCount > 0;
        }
    }
}
