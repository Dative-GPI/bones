using Akka.Actor;

namespace Demo.Core.Messages
{
    public class Request
    {
        public readonly IActorRef? Target;

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
