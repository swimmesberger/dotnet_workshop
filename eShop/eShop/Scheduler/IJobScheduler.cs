namespace eShop.Scheduler;

public interface IJobScheduler {
    Task EnqueueJob(IJob job, CancellationToken cancellationToken = default);

    Task EnqueueJob(long key, IJob job, CancellationToken cancellationToken = default);

    T? GetJobOrDefault<T>(Guid id, T? defaultValue = default) where T: class, IJob;
    
    List<T> GetJobs<T>() where T: class, IJob;
}