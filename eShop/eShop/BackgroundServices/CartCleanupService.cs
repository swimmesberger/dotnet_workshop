using eShop.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace eShop.BackgroundServices; 

public class CartCleanupService : BackgroundService {
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(20);
    
    public CartCleanupService(ILogger<CartCleanupService> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            _logger.LogInformation("CartCleanupService starting");
            using (var scope = _serviceProvider.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                // delete expired carts
                var deleteWindow = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(24));
                var toDeleteCart = await dbContext.Carts
                    .Where(c => c.LastAccess < deleteWindow)
                    .ToListAsync(cancellationToken: stoppingToken);
                dbContext.Carts.RemoveRange(toDeleteCart);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Removed {CartCount} expired carts", toDeleteCart.Count);
            }
            _logger.LogInformation("CartCleanupService finished");
            await Task.Delay(UpdateInterval, stoppingToken);
        }
    }
}