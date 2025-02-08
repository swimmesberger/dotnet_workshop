namespace ChatApp.Common;

public static class ServiceProviders {
    public static IServiceProvider Create(Func<Type, object?> @delegate, IServiceProvider? fallbackServiceProvider = null) {
        if (fallbackServiceProvider == null) {
            return new DelegateServiceProvider(@delegate);
        }
        return new CompositeServiceProvider(new DelegateServiceProvider(@delegate), fallbackServiceProvider);
    }
}
