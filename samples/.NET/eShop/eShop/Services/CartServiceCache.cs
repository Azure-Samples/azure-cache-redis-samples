using eShop.Data;
using eShop.Helpers;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace eShop.Services
{
    public class CartServiceCache : ICartService
    {
        private readonly IDistributedCache _cache;


        public CartServiceCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<Cart> AddItemToCart(string username, int itemId, decimal price, int quantity = 1)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));
            string cartItemListString = await _cache.GetStringAsync(CacheKeyConstants.GetCartItemListKey(username));
            int _cartId;

            if (cartItemListString.IsNullOrEmpty()) 
            {
                _cartId = await generateCartId();

                await _cache.SetStringAsync(_cartId.ToString(), username, options);
                await _cache.SetStringAsync(username, _cartId.ToString(), options);
                List<CartItem> cartItemList = new List<CartItem>();
                CartItem cartItemToAdd = new CartItem(itemId, quantity, price);
                cartItemToAdd.SetCartId(_cartId);
                cartItemList.Add(cartItemToAdd);
                string newCartItemString = ConvertData<CartItem>.ObjectListToString(cartItemList);
                await _cache.SetStringAsync(CacheKeyConstants.GetCartItemListKey(username),newCartItemString, options);
            }
            else 
            {
                List<CartItem> cartItemList = ConvertData<CartItem>.StringToObjectList(cartItemListString);
                CartItem cartItem = cartItemList.Where(item => item.ItemId == itemId).FirstOrDefault();
                if (cartItem != null)
                {
                    CartItem newCartItem = new CartItem(itemId, cartItem.Quantity+1, price);
                    _cartId = cartItem.Id;
                    cartItemList.Remove(cartItem);
                    cartItemList.Add(newCartItem);
                }
                else
                {
                    CartItem newCartItem = new CartItem(itemId, 1, price);
                    string cartIdString = await _cache.GetStringAsync(username);
                    if(cartIdString != null)
                    {
                        int cartId = Int32.Parse(cartIdString);
                        newCartItem.SetCartId(cartId);
                    }

                    _cartId = newCartItem.CartId;

                    cartItemList.Add(newCartItem);
                }

                string CartItemListToUpdateString = ConvertData<CartItem>.ObjectListToString(cartItemList);
                await _cache.SetStringAsync(CacheKeyConstants.GetCartItemListKey(username), CartItemListToUpdateString, options);
            }

            Cart _cart = new Cart(username);
            _cart.setId(_cartId);
            return _cart;
        }

        public async Task DeleteCartAsync(int cartId)
        {
            string username = await _cache.GetStringAsync(cartId.ToString());

            if (username == null)
            {
                return;
            }
            await _cache.RemoveAsync(cartId.ToString());
            await _cache.RemoveAsync(username);
            await _cache.RemoveAsync(CacheKeyConstants.GetCartItemListKey(username));
        }

        public async Task<Cart?> GetCartAsync(string username)
        {
            Cart cart = new Cart(username);
            string cartIdString = await _cache.GetStringAsync(username);
            if (cartIdString == null)
            {
                return null;
            }

            int cartId = Int32.Parse(cartIdString);
            cart.setId(cartId);

            return cart;

        }

        public async Task<int> GetCartId(Cart cart)
        {
            string username = cart.BuyerId.ToString();

            string cartIdString = await _cache.GetStringAsync(username);

            int cartId = Int32.Parse(cartIdString);

            return cartId;
        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public async Task TransferCartAsync(string anonymousName, string userName)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));

            string anonymousId = await _cache.GetStringAsync(anonymousName);
            if(anonymousId.IsNullOrEmpty())
            { 
                return; 
            }
            string cartItemListString = await _cache.GetStringAsync(CacheKeyConstants.GetCartItemListKey(anonymousName));
            if (!cartItemListString.IsNullOrEmpty())
            {
                await _cache.SetStringAsync(CacheKeyConstants.GetCartItemListKey(userName), cartItemListString, options);
                await _cache.SetStringAsync(anonymousId, userName);
                await _cache.SetStringAsync(userName, anonymousId);

            }

            await _cache.RemoveAsync(anonymousName);
            await _cache.RemoveAsync(CacheKeyConstants.GetCartItemListKey(anonymousName));
        }

        private async Task<int> generateCartId()
        {
            var rand = new Random();
            int cardId = rand.Next();

            string username = "placeholder";

            while (!username.IsNullOrEmpty()) 
            {
                username = await _cache.GetStringAsync(cardId.ToString());
            }

            return cardId;
        }
    }
}
