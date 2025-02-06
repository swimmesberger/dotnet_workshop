using ChatApp.Actor.Abstractions;
using ChatApp.Actor.Local;
using ChatApp.Common;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Application;

public static class ActorServiceExtensions {
    public static ActorServiceBuilder AddActorSystem(this IServiceCollection services) {
        services.AddOptions();
        services.AddSingleton<LocalActorSystem>();
        services.AddSingleton<ActorServiceScopeProvider>();
        services.AddSingleton<IActorServiceScopeProvider>(sp => sp.GetRequiredService<ActorServiceScopeProvider>());
        services.AddScoped(typeof(IStorage<>), typeof(InMemoryStorage<>));
        services.AddTransient<IActorSystem>(sp => sp.GetRequiredService<LocalActorSystem>());
        services.AddHostedService(sp => sp.GetRequiredService<LocalActorSystem>());
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
