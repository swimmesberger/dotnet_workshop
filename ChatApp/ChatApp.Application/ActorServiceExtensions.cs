using ChatApp.Application.Common;
using ChatApp.Common.Actors.Abstractions;
using ChatApp.Common.Actors.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChatApp.Application;

public static class ActorServiceExtensions {
    public static ActorServiceBuilder AddActorSystem(this IServiceCollection services) {
        services.AddOptions();
        services.AddSingleton<LocalActorRegistry>();
        services.AddSingleton<LocalActorSystem>();
        services.AddTransient<IActorSystem>(sp => sp.GetRequiredService<LocalActorSystem>());
        services.TryAddTransient<IActorServiceScopeProvider, SimpleActorServiceScopeProvider>();
        services.AddHostedService<LocalActorService>();
        services.AddSingleton(typeof(IStorage<>), typeof(InMemoryStorage<>));
        return new ActorServiceBuilder(services);
    }
}

public sealed class ActorServiceBuilder {
    private IServiceCollection Services { get; }

    public ActorServiceBuilder(IServiceCollection services) {
        Services = services;
    }

    public ActorServiceBuilder MapActor<T>() where T: class, IActor {
        Services.Configure<ActorSystemConfiguration>(x => x.RegisterActor<T>());
        Services.AddScoped<IRequiredActor<T>, RequiredActor<T>>();
        return this;
    }
}
