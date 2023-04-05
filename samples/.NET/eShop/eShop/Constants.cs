namespace eShop;

public static class Constants
{
    public const string CART_COOKIENAME = "eShop";
    public const int ITEMS_PER_PAGE = 10;
    public const string DEFAULT_USERNAME = "Guest";
    public const string CART_ID = "CartId";
    public const string UNIQUE_CACHE_TAG = "Shopping_Since";
}

public static class CacheKeyConstants
{
    public const string AllProductKey = "eShopAllProducts";
    public const string ProductPrefix = "productId_";
    public const string ShoppingCartPrefix = "cart_";
    //public static string GetShoppingCartKey(string userName)
    //{
    //    return "cart_&_" + userName;
    //}
    public static string GetCartItemListKey(string userName)
    { 
        return "cartItemList_&_" + userName;
    }
}

public static class SessionConstants
{
    public const string LastViewed = "LastViewedItem";
}