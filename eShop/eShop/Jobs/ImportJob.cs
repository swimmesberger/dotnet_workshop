using eShop.Scheduler;

namespace eShop.Jobs;

public class ImportJob : AbstractJob {
    private readonly byte[] fileData;
    
    public ImportJob(byte[] fileData) {
        this.fileData = fileData;
    }

     override public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) {
        // simulate work
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        Progress = 0.1;
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        Progress = 0.3;
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        Progress = 0.6;
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        Progress = 0.9;
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        Progress = 1;
    }
}