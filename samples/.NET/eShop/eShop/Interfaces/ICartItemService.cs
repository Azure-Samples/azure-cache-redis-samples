using eShop.Models;

namespace eShop.Interfaces;

public interface ICartItemService
{
    Task<List<CartItem>> GetCartItemAsync(int cartId);
}
