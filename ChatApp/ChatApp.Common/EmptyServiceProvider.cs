using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common;

public sealed class EmptyServiceProvider : IServiceProvider {
    public static IServiceProvider Instance { get; } = new EmptyServiceProvider();
    public static AsyncServiceScope EmptyAsyncScope => new AsyncServiceScope(new DelegateServiceScope(Instance));
    public static IServiceScope EmptyScope => new DelegateServiceScope(Instance);

    private EmptyServiceProvider() { }

    public object GetService(Type serviceType) {
        return serviceType == typeof(IServiceScopeFactory) ?
            new DelegateServiceScopeFactory(Instance) : null!;
    }

    public static AsyncServiceScope NotDisposable(IServiceProvider serviceProvider) {
        return new AsyncServiceScope(new DelegateServiceScope(serviceProvider));
    }
}
