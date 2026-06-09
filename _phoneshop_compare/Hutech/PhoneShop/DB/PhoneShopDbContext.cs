using Microsoft.EntityFrameworkCore;
using PhoneShop.Models;

namespace PhoneShop.DB
{
    public class PhoneShopDbContext : DbContext
    {
        public PhoneShopDbContext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
