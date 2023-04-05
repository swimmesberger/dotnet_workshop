namespace eShop.Scheduler; 

public abstract class AbstractJob : IJob {
    public Guid Id { get; }
    private double _progress;

    // thread-safe progress access
    protected SemaphoreSlim Lock { get; }
    public double Progress {
        get {
            Lock.Wait();
            try {
                return _progress;
            } finally {
                Lock.Release();
            }
        }
        protected set {
            Lock.Wait();
            try {
                _progress = value;
            } finally {
                Lock.Release();
            }
        }
    }

    private JobStatus _status;
    public JobStatus Status {
        get {
            Lock.Wait();
            try {
                return _status;
            } finally {
                Lock.Release();
            }
        }

        set {
            Lock.Wait();
            try {
                _status = value;
            } finally {
                Lock.Release();
            }
        }
    }

    protected AbstractJob() {
        Id = Guid.NewGuid();
        Lock = new SemaphoreSlim(1);
        _status = JobStatus.None;
        _progress = 0.0;
    }

    public abstract Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}