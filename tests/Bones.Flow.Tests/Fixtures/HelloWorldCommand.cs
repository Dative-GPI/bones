namespace Bones.Flow.Tests
{
    public class HelloWorldCommand : IRequest, IActorRequest
    {
        public string Message { get; set; } = "Hello world";

        public string ActorId { get; set; }
    }
}