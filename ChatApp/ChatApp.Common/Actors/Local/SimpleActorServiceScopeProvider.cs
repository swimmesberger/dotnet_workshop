using ChatApp.Common.Actors.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common.Actors.Local;

public sealed class SimpleActorServiceScopeProvider : IActorServiceScopeProvider {
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SimpleActorServiceScopeProvider(IServiceScopeFactory serviceScopeFactory) {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IServiceScope GetActorScope(Envelope letter, IActorOptions? options = null) => _serviceScopeFactory.CreateScope();
}
