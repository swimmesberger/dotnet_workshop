using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Domain.Services
{
    public class ActorHostedService : IHostedService
    {
        private readonly IEnumerable<IActorService> _actorServices;

        public ActorHostedService(IEnumerable<IActorService> actorServices)
        {
            _actorServices = actorServices;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var actorService in _actorServices)
            {
                await actorService.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var actorService in _actorServices)
            {
                await actorService.StopAsync(cancellationToken);
            }
        }
    }
}
