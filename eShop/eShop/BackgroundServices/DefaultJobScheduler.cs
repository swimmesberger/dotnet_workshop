using System.Collections.Concurrent;
using System.Threading.Channels;
using eShop.Scheduler;

namespace eShop.BackgroundServices;

public class DefaultJobScheduler : BackgroundService, IJobScheduler {
    private const int QueueSize = 512;
    
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly long _concurrencyLevel;
    private readonly ConcurrentDictionary<long, Channel<JobData>> _jobs;
    private readonly ConcurrentDictionary<Guid, JobData> _idToJobMap;

    public DefaultJobScheduler(ILogger<DefaultJobScheduler> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _concurrencyLevel = Environment.ProcessorCount;
        // _concurrencyLevel = 3;
        // _jobs = Channel.CreateUnbounded<JobData>();
        _jobs = new ConcurrentDictionary<long, Channel<JobData>>();
        for (var i = 0; i < _concurrencyLevel; i++) {
            _jobs[i] = Channel.CreateBounded<JobData>(QueueSize);
        }
        _idToJobMap = new ConcurrentDictionary<Guid, JobData>();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        var tasks = new Task[_concurrencyLevel];
        // create n-parallel tasks where n is the ConcurrencyLevel
        for (var i = 0; i < _concurrencyLevel; i++) {
            tasks[i] = ExecuteJobs(_jobs[i], stoppingToken);
        }
        return Task.WhenAll(tasks);
    }
    
    // execute jobs sequentially in the passed channel
    private async Task ExecuteJobs(ChannelReader<JobData> jobs, CancellationToken stoppingToken) {
        await foreach (var jobData in jobs.ReadAllAsync(stoppingToken)) {
            try {
                jobData.CancellationToken.ThrowIfCancellationRequested();
                // create outer scope for job and provide scoped service provider to job
                using (var scope = _serviceProvider.CreateScope()) {
                    _logger.LogInformation("Starting job execution of {JobType}", jobData.Job.GetType().Name);
                    jobData.Status = JobStatus.Running;
                    await jobData.Job.ExecuteAsync(scope.ServiceProvider,
                        CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, jobData.CancellationToken)
                            .Token);
                    _logger.LogInformation("Finished job execution of {JobType}", jobData.Job.GetType().Name);
                }
                jobData.TaskSource.SetResult();
            } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
                _logger.LogWarning(ex, "Canceled job execution of {JobType}", jobData.Job.GetType().Name);
                jobData.TaskSource.SetCanceled(stoppingToken);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed job execution of {JobType}", jobData.Job.GetType().Name);
                jobData.TaskSource.SetException(ex);
            } finally {
                jobData.Status = JobStatus.Finished;
                _idToJobMap.TryRemove(jobData.Id, out _);
            }
        }
    }
    
    public Task EnqueueJob(IJob job, CancellationToken cancellationToken = default) {
        return EnqueueJob(new JobData(job) {
            CancellationToken = cancellationToken
        });
    }
    
    public Task EnqueueJob(long key, IJob job, CancellationToken cancellationToken = default) {
        return EnqueueJob(new JobData(job) {
            Key = key,
            CancellationToken = cancellationToken
        });
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

    private async Task EnqueueJob(JobData jobData) {
        var jobs = GetTargetChannel(jobData);
        _idToJobMap[jobData.Id] = jobData;
        await jobs.Writer.WriteAsync(jobData);
        await jobData.Task;
    }

    private long _roundRobinNumber;

    private Channel<JobData> GetTargetChannel(JobData jobData) {
        // choose round-robin or load-balancing here
        var surrogate = jobData.Key == null ? GetNextSurrogateRoundRobin() : jobData.Key.Value % _concurrencyLevel;
        return _jobs[surrogate];
    } 

    private long GetNextSurrogateRoundRobin() {
        long initial, computed;
        do {
            initial = _roundRobinNumber;
            computed = initial + 1;
            computed = computed > _concurrencyLevel ? 1 : computed;
        } while (Interlocked.CompareExchange(ref _roundRobinNumber, computed, initial) != initial);
        return computed;
    }
    
    private long GetNextSurrogateLoadBalancing() {
        // collect number of items in channels
        IEnumerable<ChannelMetadata> channelMetadatas = _jobs.Select(entry => {
            var reader = entry.Value.Reader;
            return new ChannelMetadata(entry.Key, entry.Value, reader.CanCount ? reader.Count : int.MaxValue);
        }).OrderBy(metadata => metadata.Count); // order by number of items in channel (ascending 0, 1, 2, 3, ...)
        // take channel surrogate with the lowest item count
        return channelMetadatas.First().Surrogate;
    }

    private record JobData(IJob Job) {
        public Guid Id => Job.Id;
        public long? Key { get; init; }
        public CancellationToken CancellationToken { get; init; }
        public TaskCompletionSource TaskSource { get; } = new TaskCompletionSource();
        public Task Task => TaskSource.Task;
        public JobStatus Status {
            get {
                if (Job is AbstractJob abstractJob) {
                    return abstractJob.Status;
                }
                return JobStatus.None;
            }

            set {
                if (Job is AbstractJob abstractJob) {
                    abstractJob.Status = value;
                }
            }
        }
    }

    private record ChannelMetadata(long Surrogate, Channel<JobData> Channel, int Count);
}