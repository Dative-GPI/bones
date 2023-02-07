using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;

using Bones.Akka;
using Demo.Akka.Monitoring.Console.Abstractions;
using Demo.Akka.Monitoring.Console.Messages;

namespace Demo.Akka.Monitoring.Console.Actors
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

            var stopped = Context.ActorOf(createPong(Context), "stopActor");
            Context.Stop(stopped);

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