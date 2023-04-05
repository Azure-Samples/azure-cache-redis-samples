namespace eShop.Models;

[Serializable]
public class Cart:BaseEntity
{
    public string BuyerId { get; private set; }
    private readonly List<CartItem> _items = new List<CartItem>();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();


    public int TotalItems => _items.Sum(i => i.Quantity);

    public Cart(string buyerId)
    {
        BuyerId = buyerId;
    }


    public void AddItem(int itemId, decimal unitPrice, int quantity = 1)
    {
        if (!Items.Any(i => i.ItemId == itemId))
        {
            _items.Add(new CartItem(itemId, quantity, unitPrice));
            return;
        }
        var existingItem = Items.First(i => i.ItemId == itemId);
        existingItem.AddQuantity(quantity);
    }

    public void CopyItem(CartItem item)
    {
        _items.Add(item);
    }

    public void RemoveEmptyItems()
    {
        _items.RemoveAll(i => i.Quantity == 0);
    }

    public void SetNewBuyerId(string buyerId)
    {
        BuyerId = buyerId;
    }

    public void setId(int id)
    { 
        this.Id = id;
    }

}
