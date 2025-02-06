namespace ChatApp.Actor.Abstractions;

public interface IActorSystem {
    IActorRef? GetActor<T>() where T : IActor;
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
