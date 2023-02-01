
using Akka.Actor;
using Demo.Akka.Monitoring.Console.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Bones.Akka.Monitoring;
using Demo.Akka.Monitoring.Console.Messages;

namespace Demo.Akka.Monitoring.Console.Actors
{
    public class PongActor : MonitoredReceiveActor, IPongActor
    {
        private readonly ILogger<PongActor> _logger;
        public PongActor(IServiceProvider sp): base(sp)
        {
            _logger = sp.GetRequiredService<ILogger<PongActor>>();
            _logger.LogInformation("PongActor created");
            MonitoredReceiveAsync<Request>(async m =>
            {
                await Task.Delay(new Random().Next() % 2000);
                Context.Sender.Tell(Response.Instance);
            });
        }
    }
}
