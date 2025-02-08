namespace ChatApp.Application.Common;

public sealed class InMemoryStorage<T> : IStorage<T> where T: class {
    private T? _savedState;
    public bool RecordExists { get; private set; }

    public T State { get; set; } = null!;

    public InMemoryStorage() {
        ClearState();
    }

    public ValueTask ReadStateAsync(CancellationToken cancellationToken = default) {
        if (RecordExists) {
            State = _savedState!;
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SaveStateAsync(CancellationToken cancellationToken = default) {
        _savedState = State;
        RecordExists = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearStateAsync(CancellationToken cancellationToken = default) {
        ClearState();
        return ValueTask.CompletedTask;
    }

    private void ClearState() {
        _savedState = null;
        RecordExists = false;
        State = Activator.CreateInstance<T>();
    }
}
