using Akka.Actor;

namespace Bones.Akka.Monitoring.Console.Messages
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
