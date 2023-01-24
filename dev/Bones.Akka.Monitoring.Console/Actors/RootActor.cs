using Akka.Actor;
using Bones.Akka.Monitoring.Console.Messages;
using Microsoft.Extensions.DependencyInjection;
using Test.Abstractions;

namespace Bones.Akka.Monitoring.Console.Actors
{
    public class RootActor : ReceiveActor
    {
        public RootActor(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();

            var createPing = scope.ServiceProvider.GetRequiredService<Creator<IPingActor>>();
            var createPong = scope.ServiceProvider.GetRequiredService<Creator<IPongActor>>();

            var pong = Context.ActorOf(createPong(Context), "ponga");
            var ping = Context.ActorOf(createPing(Context), "pinga");
            
            ping.Tell(new Request(pong));
            
            Started();
        }

        private void Started()
        {
        }

        protected override void PostStop()
        {
        }

    }
}