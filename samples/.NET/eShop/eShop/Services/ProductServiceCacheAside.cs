using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Text;
using System.Text.Json;
using eShop.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;

namespace eShop.Services
{
    public class ProductServiceCacheAside : IProductService
    {
        private readonly IDistributedCache _cache;
        private readonly eShopContext _context;

        public ProductServiceCacheAside(IDistributedCache cache, eShopContext context) 
        { 
            _cache = cache;
            _context = context;
        }

        public async Task AddProduct(Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
        }

        public async Task DeleteProrduct(int productId)
        {
            if (_context.Product == null)
            {
                throw new Exception("Item not found");
            }
            var product = await _context.Product.FindAsync(productId);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
            await _cache.RemoveAsync(CacheKeyConstants.ProductPrefix + productId);
        }

        public async Task EditProduct(Product product)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
                await _cache.RemoveAsync(CacheKeyConstants.ProductPrefix + product.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    throw new Exception("Item not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var stringFromCache = await _cache.GetStringAsync(CacheKeyConstants.AllProductKey);
            if (stringFromCache.IsNullOrEmpty())
            {
                if (_context.Product == null) throw new Exception("Entity set 'eShopContext.Product'  is null.");
                List<Product> AllProductList = await Task.Run(() => _context.Product.ToList());
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30)).SetAbsoluteExpiration(TimeSpan.FromDays(30));
                string AllProductString = ConvertData<Product>.ObjectListToString(AllProductList);
                await _cache.SetStringAsync(CacheKeyConstants.AllProductKey, AllProductString, options);
                return ConvertData<Product>.StringToObjectList(AllProductString);
            }


            return ConvertData<Product>.StringToObjectList(stringFromCache);

            //return await _context.Product.ToListAsync();
         }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var stringFromCache = await _cache.GetStringAsync(CacheKeyConstants.ProductPrefix + productId);
            if (stringFromCache.IsNullOrEmpty())
            {
                var productById = await Task.Run(() => _context.Product.Where(product => product.Id == productId).FirstOrDefault());
                if (productById == null)
                {
                    return null;
                }
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30)).SetAbsoluteExpiration(TimeSpan.FromDays(30));
                string ProductByIdString = ConvertData<Product>.ObjectToString(productById);
                await _cache.SetStringAsync(CacheKeyConstants.ProductPrefix + productId, ProductByIdString, options);
                return ConvertData<Product>.StringToObject(ProductByIdString);
            }

            return ConvertData<Product>.StringToObject(stringFromCache);
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }


}
