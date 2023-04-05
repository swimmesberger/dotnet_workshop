using System.Collections.Concurrent;
using System.Threading.Channels;
using eShop.Scheduler;

namespace eShop.BackgroundServices; 

/**
 * Simple single "threaded" implementation for a job scheduler
 */
public class SingleJobScheduler : BackgroundService, IJobScheduler {
    private const int QueueSize = 512;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<JobData> _jobs;
    private readonly ConcurrentDictionary<Guid, JobData> _idToJobMap;

    public SingleJobScheduler(ILogger<SingleJobScheduler> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jobs = Channel.CreateBounded<JobData>(QueueSize);
        _idToJobMap = new ConcurrentDictionary<Guid, JobData>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await foreach (var jobData in _jobs.Reader.ReadAllAsync(stoppingToken)) {
            try {
                // create outer scope for job and provide scoped service provider to job
                using var scope = _serviceProvider.CreateScope();
                _logger.LogInformation("Starting job execution of {JobType}", jobData.Job.GetType().Name);
                await jobData.Job.ExecuteAsync(scope.ServiceProvider, 
                    CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, jobData.CancellationToken).Token);
                _logger.LogInformation("Finished job execution of {JobType}", jobData.Job.GetType().Name);
            } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
                _logger.LogWarning(ex, "Canceled job execution of {JobType}", jobData.Job.GetType().Name);
                jobData.TaskSource.SetCanceled(stoppingToken);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed job execution of {JobType}", jobData.Job.GetType().Name);
                jobData.TaskSource.SetException(ex);
            } finally {
                _idToJobMap.TryRemove(jobData.Id, out _);
            }
        }
    }

    public Task EnqueueJob(long key, IJob job, CancellationToken cancellationToken = default)
        => EnqueueJob(job, cancellationToken);

    public async Task EnqueueJob(IJob job, CancellationToken cancellationToken = default) {
        var jobData = new JobData(job) {
            CancellationToken = cancellationToken
        };
        _idToJobMap[jobData.Id] = jobData;
        await _jobs.Writer.WriteAsync(jobData, cancellationToken);
        await jobData.Task;
    }

    public T? GetJobOrDefault<T>(Guid id, T? defaultValue = default) where T: class, IJob {
        return _idToJobMap.TryGetValue(id, out var jobData) ? jobData.Job as T : defaultValue;
    }

    public List<T> GetJobs<T>() where T : class, IJob {
        return _idToJobMap.Values
            .Select(data => data.Job)
            .Where(job => job is T)
            .Cast<T>()
            .ToList();
    }

    private record JobData(IJob Job) {
        public Guid Id => Job.Id;
        public CancellationToken CancellationToken { get; init; }
        public TaskCompletionSource TaskSource { get; } = new TaskCompletionSource();
        public Task Task => TaskSource.Task;
    }
}