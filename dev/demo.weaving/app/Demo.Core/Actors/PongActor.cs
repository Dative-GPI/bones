
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Demo.Domain.Abstractions;
using Demo.Core.Messages;

namespace Demo.Core.Actors
{
    public class PongActor : ReceiveActor, IPongActor
    {
        private readonly ILogger<PongActor> _logger;
        public PongActor(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<PongActor>>();
            _logger.LogInformation("PongActor created");
            ReceiveAsync<Request>(async m =>
            {
                await Task.Delay(new Random().Next() % 2000);
                Context.Sender.Tell(Response.Instance);
            });
        }
    }
}
