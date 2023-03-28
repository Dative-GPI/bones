using Bones.Flow;

namespace Demo.Domain.Abstractions
{
    public interface IActorRequest : IRequest
    {
        string ActorId { get; }
    }
}