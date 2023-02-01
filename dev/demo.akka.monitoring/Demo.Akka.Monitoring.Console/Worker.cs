using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bones.Akka;
using Bones.Akka.Monitoring;

using Demo.Akka.Monitoring.Console.Actors;

namespace Demo.Akka.Monitoring.Console
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