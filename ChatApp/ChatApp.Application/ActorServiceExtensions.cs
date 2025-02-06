using ChatApp.Common.Actors;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Application;

public static class ActorServiceExtensions {
    public static ActorServiceBuilder AddActorSystem(this IServiceCollection services) {
        services.AddHostedService<ActorSystem>();
        return new ActorServiceBuilder(services);
    }

    public sealed class ActorServiceBuilder {
        private readonly IServiceCollection _services;

        public ActorServiceBuilder(IServiceCollection services) {
            _services = services;
        }

        public ActorServiceBuilder MapActor<T>() where T: IActor {
            _services.AddScoped<IRequiredActor<T>, RequiredActor<T>>();
            return this;
        }
    }
}
