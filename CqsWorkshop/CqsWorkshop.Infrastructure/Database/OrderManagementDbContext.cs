using CqsWorkshop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CqsWorkshop.Infrastructure.Database;

public class OrderManagementDbContext : DbContext {
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    public OrderManagementDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityTypeConfiguration());
    }

    private class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order> {
        public void Configure(EntityTypeBuilder<Order> builder) {
            builder.HasKey(o => o.Id).IsClustered(false);
            builder.HasIndex(o => o.OrderDate).IsClustered();
            builder.Property(o => o.TotalPrice).HasPrecision(18, 2);
        }
    }
    
    private class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer> {
        public void Configure(EntityTypeBuilder<Customer> builder) {
            builder.HasKey(c => c.Id).IsClustered(false);
            builder.HasIndex(c => c.CreatedAt).IsClustered();
            builder.Property(c => c.TotalRevenue).HasPrecision(18, 2);
            builder.OwnsOne(c => c.Address);
        }
    }
}