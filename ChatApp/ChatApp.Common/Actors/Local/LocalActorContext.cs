using ChatApp.Common.Actors.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common.Actors.Local;

public sealed class LocalActorContext : IActorContext {
    public required LocalActorSystem ActorSystem { get; init; }
    IActorSystem IActorContext.ActorSystem => ActorSystem;
    public LocalActorSystem ActorFactory => ActorSystem;
    public LocalActorRegistry ActorRegistry => ActorSystem.ActorRegistry;

    public required ActorConfiguration Configuration { get; init; }
    public string? Id => Configuration.Id;
    public Type ActorType => Configuration.ActorType;
    public IActorRef Self { get; set; } = IActorRef.Nobody;
    public Envelope Letter { get; set; } = Envelope.Unknown;
    IEnvelope IActorContext.Letter => Letter;
    public CancellationToken RequestAborted { get; set; } = CancellationToken.None;
    public IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    internal IActorServiceScopeProvider ActorServiceScopeProvider { get; init; } =
        EmptyActorServiceScopeProvider.Instance;

    internal IServiceScope? RequestServiceScope { get; set; }
    public IServiceProvider RequestServices {
        get {
            RequestServiceScope ??= ActorServiceScopeProvider.GetActorScope(Letter, Configuration.Options!);
            return RequestServiceScope.ServiceProvider;
        }
    }

    public ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor => ActorSystem.GetOrCreateActorAsync(configuration, cancellationToken);

    public IActorRef? GetActor<T>(string? id = null) where T : IActor => ActorSystem.GetActor<T>(id);

    public ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default) where T : IActor => ActorSystem.GetActorAsync<T>(id, cancellationToken);
}
