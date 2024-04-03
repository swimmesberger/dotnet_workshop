namespace CoolNewProject.Domain;

public sealed class AsyncLock {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public SemaphoreSlimDisposable Wait() {
        _semaphore.Wait();
        return Release(_semaphore);
    }

    public ValueTask<SemaphoreSlimDisposable> WaitAsync(CancellationToken cancellationToken = default) {
        return WaitAsync(null, cancellationToken);
    }

    public async ValueTask<SemaphoreSlimDisposable> WaitAsync(TimeSpan? timeout, CancellationToken cancellationToken = default) {
        timeout ??= Timeout.InfiniteTimeSpan;
        await _semaphore.WaitAsync(timeout.Value, cancellationToken);
        return Release(_semaphore);
    }

    private static SemaphoreSlimDisposable Release(SemaphoreSlim semaphoreSlim) => new SemaphoreSlimDisposable(semaphoreSlim);
}

public readonly struct SemaphoreSlimDisposable : IDisposable {
    private readonly SemaphoreSlim _semaphore;

    public SemaphoreSlimDisposable(SemaphoreSlim semaphore) {
        _semaphore = semaphore;
    }

    public void Dispose() => _semaphore.Release();
}
