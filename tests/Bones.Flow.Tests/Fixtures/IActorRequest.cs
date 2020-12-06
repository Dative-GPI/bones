namespace Bones.Flow.Tests
{
    public interface IActorRequest : IRequest
    {
        string ActorId { get; }
    }
}