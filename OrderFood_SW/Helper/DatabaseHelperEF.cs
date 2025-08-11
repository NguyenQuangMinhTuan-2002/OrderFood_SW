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

        public DbSet<Users> Users { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.DishId });

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Dish)
                .WithMany(d => d.OrderDetails)
                .HasForeignKey(od => od.DishId);
        }

    }
}
