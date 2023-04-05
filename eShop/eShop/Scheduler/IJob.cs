namespace eShop.Scheduler;

public interface IJob {
    Guid Id { get; }
    
    Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}