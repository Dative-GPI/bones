
using Akka.Actor;
using Demo.Core.Messages;
using Demo.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo.Core.Actors
{
    public class PingActor : ReceiveActor, IPingActor
    {
        private readonly ILogger<PingActor> _logger;
        public PingActor(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<PingActor>>();
            _logger.LogInformation("PingActor created");

            Receive<Request>(m =>
            {
                m.Target.Tell(m);
                return;
            });
            ReceiveAsync<Response>(async m =>
            {
                await Task.Delay(new Random().Next() % 800);
                Context.Sender.Tell(new Request());
            });
        }

        protected override void PreStart()
        {
            base.PreStart();
        }

        protected override void PostStop()
        {
            base.PostStop();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
        }
    }
}