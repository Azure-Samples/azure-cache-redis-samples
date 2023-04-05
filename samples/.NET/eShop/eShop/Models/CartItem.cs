using System;

namespace eShop.Models;

[Serializable]
public class CartItem : BaseEntity
{
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int ItemId { get; private set; }
    //public int CartId { get; private set; }
    public int CartId { get; set; }

    public CartItem(int itemId, int quantity, decimal unitPrice)
    {
        ItemId = itemId;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }

    public void SetCartId(int _cardId)
    {
        CartId = _cardId;
    }

}
