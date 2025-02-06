namespace ChatApp.Domain.Services;

public interface IActorService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
