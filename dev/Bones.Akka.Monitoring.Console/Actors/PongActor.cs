
using Akka.Actor;
using Bones.Akka.Monitoring.Console.Messages;
using Bones.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Abstractions;

namespace Bones.Akka.Monitoring.Console.Actors
{
    public class PongActor : DativeReceiveActor, IPongActor
    {
        private readonly ILogger<PongActor> _logger;
        public PongActor(IServiceProvider sp): base(sp)
        {
            _logger = sp.GetRequiredService<ILogger<PongActor>>();
            _logger.LogInformation("PongActor created");
            ReceiveAsyncMonitored<Request>(async m =>
            {
                await Task.Delay(new Random().Next() % 2000);
                Context.Sender.Tell(Response.Instance);
            });
        }
    }
}
