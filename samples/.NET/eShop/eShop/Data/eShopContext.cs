using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eShop.Models;

namespace eShop.Data
{
    public class eShopContext : DbContext
    {
        public eShopContext (DbContextOptions<eShopContext> options)
            : base(options)
        {
        }

        public DbSet<eShop.Models.Product> Product { get; set; } = default!;
        public DbSet<eShop.Models.CartItem> cartItems { get; set; } = default!;
        public DbSet<eShop.Models.Cart> carts { get; set; } = default!;
    }
}
