using Bones.Flow;

namespace Demo.Akka.Monitoring.Console.Abstractions
{
    public interface IActorRequest : IRequest
    {
        string ActorId { get; }
    }
}