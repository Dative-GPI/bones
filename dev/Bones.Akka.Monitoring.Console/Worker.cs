using Akka.Actor;
using Bones.Akka.Monitoring.Console.Actors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bones.Akka.Monitoring.Console
{
    public class Worker : IHostedService
    {
        private IServiceProvider _services;

        public Worker(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {            
                var logger = _services.GetRequiredService<ILogger<Program>>();

                var meter = _services.GetRequiredService<AkkaMonitor>();

                var system = _services.GetRequiredService<ActorSystem>();

                var rootCreator = _services.GetRequiredService<RootCreator<RootActor>>();

                var root = system.ActorOf(rootCreator(system), "root");

                return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}