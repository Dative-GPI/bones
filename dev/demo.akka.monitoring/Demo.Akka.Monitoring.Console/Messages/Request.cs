using Akka.Actor;

namespace Demo.Akka.Monitoring.Console.Messages
{
    public class Request
    {
        public readonly IActorRef Target;

        public Request()
        {
            Target = null;
        }

        public Request(IActorRef target)
        {
            Target = target;
        }
    }
}
