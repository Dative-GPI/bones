
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Demo.Akka.Monitoring.Console.Abstractions;
using Bones.Akka.Monitoring;
using Demo.Akka.Monitoring.Console.Messages;
using System;
using System.Threading.Tasks;

namespace Demo.Akka.Monitoring.Console.Actors
{
    public class PingActor : MonitoredReceiveActor, IPingActor
    {
        private readonly ILogger<PingActor> _logger;
        public PingActor(IServiceProvider sp) : base(sp)
        {
            _logger = sp.GetRequiredService<ILogger<PingActor>>();
            _logger.LogInformation("PingActor created");

            MonitoredReceive<Request>(m =>
            {
                m.Target.Tell(m);
                return;
            });
            MonitoredReceiveAsync<Response>(async m =>
            {
                await Task.Delay(new Random().Next() % 800);
                Context.Sender.Tell(new Request());
            });
        }
    }
}