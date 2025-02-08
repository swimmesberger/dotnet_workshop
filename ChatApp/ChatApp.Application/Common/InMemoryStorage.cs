namespace ChatApp.Application.Common;

public sealed class InMemoryStorage<T> : IStorage<T> where T: class {
    private T? _savedState;
    public bool RecordExists { get; private set; }

    public T? State { get; set; }

    public InMemoryStorage() {
        State = Activator.CreateInstance<T>();
        RecordExists = false;
    }

    public ValueTask ReadStateAsync(CancellationToken cancellationToken = default) {
        State = _savedState;
        return ValueTask.CompletedTask;
    }

    public ValueTask SaveStateAsync(CancellationToken cancellationToken = default) {
        _savedState = State;
        RecordExists = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearStateAsync(CancellationToken cancellationToken = default) {
        _savedState = null;
        RecordExists = false;
        State = _savedState;
        return ValueTask.CompletedTask;
    }
}
