namespace ChatApp.Application.Common;

public interface IStorage<T> {
    T? State { get; set; }
    bool RecordExists { get; }

    ValueTask ReadStateAsync(CancellationToken cancellationToken = default);
    ValueTask SaveStateAsync(CancellationToken cancellationToken = default);
    ValueTask ClearStateAsync(CancellationToken cancellationToken = default);
}
