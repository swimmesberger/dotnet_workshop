namespace ChatApp.Common.Grains;

public interface IGrain {
    ValueTask OnActivate(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    ValueTask OnDeactivate(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
