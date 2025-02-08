namespace ChatApp.Application;

public sealed record ClientRequestOptions {
    public IReadOnlyDictionary<string, object>? Headers { get; init; }
}
