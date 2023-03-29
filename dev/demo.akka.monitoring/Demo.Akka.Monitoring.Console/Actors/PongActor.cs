
using Akka.Actor;
using Demo.Akka.Monitoring.Console.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Bones.Akka.Monitoring;
using Demo.Akka.Monitoring.Console.Messages;
using Bones.Flow;
using Demo.Akka.Monitoring.Console.Commands;
using System;
using System.Threading.Tasks;

namespace Demo.Akka.Monitoring.Console.Actors
{
    public class PongActor : MonitoredReceiveActor, IPongActor
    {
        private readonly ILogger<PongActor> _logger;
        private readonly ICommandHandler<HelloWorldCommand> _pipeline;
        public PongActor(IServiceProvider sp): base(sp)
        {
            _pipeline = sp.GetRequiredService<ICommandHandler<HelloWorldCommand>>();

            _logger = sp.GetRequiredService<ILogger<PongActor>>();
            _logger.LogInformation("PongActor created");
            MonitoredReceiveAsync<Request>(async m =>
            {
                await Task.Delay(new Random().Next() % 2000);

                #region Bones.Flow
                await _pipeline.HandleAsync(new HelloWorldCommand
                {
                    ActorId = Self.Path.Name
                });
                #endregion

                Context.Sender.Tell(Response.Instance);
            });
        }
    }
}
