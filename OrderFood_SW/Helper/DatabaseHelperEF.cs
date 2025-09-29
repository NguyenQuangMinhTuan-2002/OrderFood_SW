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

        public DbSet<TaxRate> TaxRates { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationReads> NotificationReads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.OrderDetailId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Dish)
                .WithMany(d => d.OrderDetails)
                .HasForeignKey(od => od.DishId);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.OrderDetailId)
                .ValueGeneratedOnAdd();

            // Configure Notification entity
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Content)
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.SenderId)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.SenderName)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            //modelBuilder.Entity<Notification>()
            //    .Property(n => n.IsRead)
            //    .HasDefaultValue(false);

            modelBuilder.Entity<Notification>()
                .Property(n => n.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Priority)
                .HasDefaultValue("Normal");

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasDefaultValue("General");
        }

    }
}
