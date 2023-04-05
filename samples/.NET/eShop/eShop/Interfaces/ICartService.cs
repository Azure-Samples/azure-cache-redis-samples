using eShop.Models;
using NuGet.ContentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Interfaces;

public interface ICartService
{
    Task TransferCartAsync(string anonymousId, string userName);
    Task<Cart> AddItemToCart(string username, int catalogItemId, decimal price, int quantity = 1);
    Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities);
    Task DeleteCartAsync(int cartId);
    Task<Cart?> GetCartAsync(string cartId);
    Task<int> GetCartId(Cart cart);

}
