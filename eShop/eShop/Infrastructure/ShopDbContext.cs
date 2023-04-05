using eShop.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure;

public class ShopDbContext : DbContext {
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var testCart = new Cart() {
            LastAccess = DateTimeOffset.UtcNow
        };
        var testProduct = new Product() {
            Name = "Test Product",
            Price = 10.50M
        };
        var testCartItem = new CartItem() {
            CartId = testCart.Id,
            ProductId = testProduct.Id,
            Amount = 5
        };
        
        var cart = modelBuilder.Entity<Cart>();
        cart.HasKey(c => c.Id).IsClustered(false);
        cart.HasData(testCart);
        
        var cartItem = modelBuilder.Entity<CartItem>();
        cartItem.HasKey(c => c.Id).IsClustered(false);
        cartItem.HasData(testCartItem);

        var product = modelBuilder.Entity<Product>();
        product.HasKey(c => c.Id).IsClustered(false);
        product.Property(p => p.Price).HasPrecision(18,2);
        product.HasData(testProduct);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (optionsBuilder.IsConfigured) return;
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var connectionString = configuration.GetConnectionString("Main");
        optionsBuilder.UseSqlServer(connectionString);
    }
}