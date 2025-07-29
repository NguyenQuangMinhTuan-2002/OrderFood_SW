// Helpers/DatabaseHelperTest.cs
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Models;

namespace OrderFood_SW.Helper
{
    public class DatabaseHelperEF : DbContext
    {
        public DatabaseHelperEF(DbContextOptions<DatabaseHelperEF> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Table> Tables { get; set; }

        public DbSet<Dish> Dishes { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
