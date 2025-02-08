namespace ChatApp.Common;

public sealed class CompositeServiceProvider : IServiceProvider {
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceProvider _fallbackServiceProvider;

    public CompositeServiceProvider(IServiceProvider serviceProvider, IServiceProvider fallbackServiceProvider) {
        _serviceProvider = serviceProvider;
        _fallbackServiceProvider = fallbackServiceProvider;
    }

    public object? GetService(Type serviceType) {
        var service = _serviceProvider.GetService(serviceType);
        return service ?? _fallbackServiceProvider.GetService(serviceType);
    }
}
