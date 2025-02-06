using ChatApp.Common;

namespace ChatApp.Actor.Local;

public sealed class InMemoryStorage<T> : IStorage<T> {
    private T _internalState;
    public bool RecordExists { get; private set; }

    public T State { get; set; }

    public InMemoryStorage() {
        _internalState = Activator.CreateInstance<T>();
        State = _internalState;
    }

    public ValueTask ReadStateAsync(CancellationToken cancellationToken = default) {
        State = _internalState;
        return ValueTask.CompletedTask;
    }

    public ValueTask SaveStateAsync(CancellationToken cancellationToken = default) {
        _internalState = State;
        RecordExists = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearStateAsync(CancellationToken cancellationToken = default) {
        _internalState = default!;
        RecordExists = false;
        return ValueTask.CompletedTask;
    }
}
