using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common;

public sealed class DelegateServiceProvider : IServiceProvider {
    public static readonly DelegateServiceProvider Empty = new DelegateServiceProvider(type => null);

    private readonly Func<Type, object?> _delegate;

    public DelegateServiceProvider(Func<Type, object?> @delegate) {
        _delegate = @delegate;
    }

    public object? GetService(Type serviceType) => _delegate.Invoke(serviceType);
}

public sealed class DelegateServiceScopeFactory : IServiceScopeFactory {
    public Func<IServiceProvider> ServiceProvider { get; }

    public DelegateServiceScopeFactory(IServiceProvider serviceProvider) : this(() => serviceProvider) { }

    public DelegateServiceScopeFactory(Func<IServiceProvider> serviceProvider) {
        ServiceProvider = serviceProvider;
    }

    public IServiceScope CreateScope() => new DelegateServiceScope(ServiceProvider.Invoke());
}

public sealed class DelegateServiceScope : IServiceScope {
    public IServiceProvider ServiceProvider { get; }

    public DelegateServiceScope(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;
    }

    public void Dispose() { }
}
