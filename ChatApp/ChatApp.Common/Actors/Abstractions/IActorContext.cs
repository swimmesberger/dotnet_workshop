namespace ChatApp.Common.Actors.Abstractions;

public interface IActorContext {
    string? Id { get; }

    Type ActorType { get; }

    IActorSystem ActorSystem { get; }

    IEnvelope Letter { get; }

    IServiceProvider RequestServices { get; }

    CancellationToken RequestAborted { get; }

    IActorRef Self { get; }

    /// <summary>
    /// Gets or sets a key/value collection that can be used to share data within the scope of this request.
    /// </summary>
    IDictionary<object, object?> Items { get;  }
}
