using System.Collections.Concurrent;
using ChatApp.Actor.Abstractions;
using ChatApp.Actor.Local;
using ChatApp.Common;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Application;

public sealed class ActorServiceScopeProvider : IActorServiceScopeProvider {
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ConcurrentDictionary<string, IServiceProvider> _serviceProviders;

    public ActorServiceScopeProvider(IServiceScopeFactory serviceScopeFactory) {
        _serviceScopeFactory = serviceScopeFactory;
        _serviceProviders = new ConcurrentDictionary<string, IServiceProvider>();
    }

    public IDisposable SetServiceProvider(string requestId, IServiceProvider serviceProvider) {
        _serviceProviders[requestId] = serviceProvider;
        return Disposable.From(() => _serviceProviders.Remove(requestId, out _));
    }

    public IServiceScope GetActorScope(Envelope letter, ActorOptions options) {
        if (letter.RequestId != null && _serviceProviders.TryGetValue(letter.RequestId, out var serviceProvider)) {
            // discard dispose functionality to prevent double dispose
            return new DelegateServiceScope(serviceProvider);
        }
        return _serviceScopeFactory.CreateScope();
    }
}
