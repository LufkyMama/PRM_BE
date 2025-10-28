using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;
using PRM_BE.Model.Enums;

namespace PRM_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Flower> Flowers => Set<Flower>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== USER ==========
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(255);
                e.Property(x => x.Password).IsRequired().HasMaxLength(255);
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.UserName).IsUnique();

                e.HasMany(x => x.Orders)
                 .WithOne(o => o.CustomerUser)
                 .HasForeignKey(o => o.CustomerUserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ========== FLOWER ==========
            modelBuilder.Entity<Flower>(e =>
            {
                e.ToTable("Flowers");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
                e.Property(x => x.ImageUrl).IsRequired().HasMaxLength(512);

                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Stock).HasDefaultValue(0);

                // Category hiện đang là string trong model; để đồng bộ tốt hơn có thể đổi sang enum FlowerCategory sau.
                e.Property(x => x.Category).IsRequired().HasMaxLength(100);

                e.HasIndex(x => x.Name);
                e.HasIndex(x => x.Category);
            });

            // ========== ORDER ==========
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);

                e.Property(x => x.SenderName).IsRequired().HasMaxLength(120);
                e.Property(x => x.SenderEmail).IsRequired().HasMaxLength(255);
                e.Property(x => x.SenderPhone).IsRequired().HasMaxLength(20);

                e.Property(x => x.Recipient).HasMaxLength(200);

                e.Property(x => x.DeliveryDate).IsRequired();

                e.Property(x => x.ShippingFee).HasPrecision(18, 2);
                e.Property(x => x.Total).HasPrecision(18, 2);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.CreatedAt);

                // 1 - N: Order -> OrderItem
                e.HasMany(x => x.Items)
                 .WithOne(i => i.Order)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                // 1 - 1: Order -> Payment
                e.HasOne(x => x.Payment)
                 .WithOne(p => p.Order)
                 .HasForeignKey<Payment>(p => p.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                // 1 - 1: Order -> Delivery
                e.HasOne(x => x.Delivery)
                 .WithOne(d => d.Order)
                 .HasForeignKey<Delivery>(d => d.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ORDER ITEM ==========
            modelBuilder.Entity<OrderItem>(e =>
            {
                e.ToTable("OrderItems");
                e.HasKey(x => x.Id);

                e.Property(x => x.UnitPrice).HasPrecision(18, 2);
                e.Property(x => x.LineTotal)
                 .HasPrecision(18, 2)
                 // Generated column trên PostgreSQL: UnitPrice * Quantity
                 .HasComputedColumnSql("\"UnitPrice\" * \"Quantity\"", stored: true);

                // N - 1: OrderItem -> Flower
                e.HasOne(x => x.Flower)
                 .WithMany()
                 .HasForeignKey(x => x.FlowerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.OrderId, x.FlowerId });
            });

            // ========== PAYMENT ==========
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("Payments");
                e.HasKey(x => x.Id);

                e.Property(x => x.Method).HasConversion<int>();
                e.Property(x => x.Status).HasConversion<int>();

                e.Property(x => x.Amount).HasPrecision(18, 2);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                e.Property(x => x.FailureReason).HasMaxLength(500);

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.OrderId).IsUnique(); // 1-1 với Order
            });

            // ========== DELIVERY ==========
            modelBuilder.Entity<Delivery>(e =>
            {
                e.ToTable("Deliveries");
                e.HasKey(x => x.Id);

                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.DeliveredAt);
                e.Property(x => x.ProofPhotoUrl).HasMaxLength(512);

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.OrderId).IsUnique(); // 1-1 với Order
            });

            // ========== ENUM CONVERSIONS (nếu cần rõ ràng) ==========
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryTimeWindow)
                .HasConversion<int>();
        }
    }
}
