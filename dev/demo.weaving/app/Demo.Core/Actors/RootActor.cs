using Akka.Actor;
using Bones.Akka;
using Demo.Core.Messages;
using Demo.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo.Core.Actors
{
    public class RootActor : ReceiveActor
    {
        private ILogger<RootActor> _logger;
        private Creator<IPingActor> _createPing;
        private Creator<IPongActor> _createPong;
        private IUntypedActorContext _context;
        public RootActor(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();

            _logger = scope.ServiceProvider.GetRequiredService<ILogger<RootActor>>();

            _createPing = scope.ServiceProvider.GetRequiredService<Creator<IPingActor>>();
            _createPong = scope.ServiceProvider.GetRequiredService<Creator<IPongActor>>();

            _context = Context;

            Started();
        }

        private void Started()
        {
            var pong = _context.ActorOf(_createPong(_context), "pong");
            var ping = _context.ActorOf(_createPing(_context), "ping");
            ping.Tell(new Request(pong));
        }

    }
}