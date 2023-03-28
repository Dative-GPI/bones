using Bones.Flow;
using Demo.Domain.Abstractions;

namespace Demo.Core.Commands
{
    public class HelloWorldCommand : IRequest, IActorRequest
    {
        public string Message { get; set; } = "Hello world";
        public string ActorId { get; set; }
    }
}