using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eShop.Models;

[Serializable]
public class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    [Range(1, 10000)]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [StringLength(60, MinimumLength = 3)]
    [Required]
    public string? Brand { get; set; }

    public string? Image { get; set; }

    public string? category { get; set; }
}

public enum Categories
{ 
    Purse,
    Shoes
}