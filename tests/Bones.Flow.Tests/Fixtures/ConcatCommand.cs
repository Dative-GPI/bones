namespace Bones.Flow.Tests
{
    public class ConcatCommand : IActorRequest, IRequest<string>
    {
        public string ActorId { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
    }
}