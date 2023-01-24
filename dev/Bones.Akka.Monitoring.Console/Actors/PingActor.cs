
using Akka.Actor;
using Bones.Akka.Monitoring.Console.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Abstractions;

namespace Bones.Akka.Monitoring.Console.Actors
{
    public class PingActor : DativeReceiveActor, IPingActor
    {
        private readonly ILogger<PingActor> _logger;
        public PingActor(IServiceProvider sp) : base(sp)
        {
            _logger = sp.GetRequiredService<ILogger<PingActor>>();
            _logger.LogInformation("PingActor created");

            ReceiveMonitored<Request>(m =>
            {
                m.Target.Tell(m);
                return;
            });
            ReceiveAsyncMonitored<Response>(async m =>
            {
                await Task.Delay(new Random().Next() % 800);
                Context.Sender.Tell(new Request());
            });
        }
    }
}