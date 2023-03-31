
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Demo.Domain.Abstractions;
using Demo.Core.Messages;
using Bones.Flow;
using Demo.Core.Commands;

namespace Demo.Core.Actors
{
    public class PongActor : ReceiveActor, IPongActor
    {
        private readonly ILogger<PongActor> _logger;
        private readonly IServiceProvider _sp;
        public PongActor(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<PongActor>>();
            _logger.LogInformation("PongActor created");

            _sp = sp;

            ReceiveAsync<Request>(async m =>
            {
                await Task.Delay(new Random().Next() % 2000);

                #region Bones.Flow
                using var scope = _sp.CreateScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<ICommandHandler<HelloWorldCommand>>();
                await pipeline.HandleAsync(new HelloWorldCommand
                {
                    ActorId = Self.Path.Name
                });
                #endregion

                Context.Sender.Tell(Response.Instance);
            });
        }
    }
}
