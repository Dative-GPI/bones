using Bones.Flow;
using Demo.Akka.Monitoring.Console.Abstractions;

namespace Demo.Akka.Monitoring.Console.Commands
{
    public class HelloWorldCommand : IRequest, IActorRequest
    {
        public string Message { get; set; } = "Hello world";
        public string ActorId { get; set; }
    }
}