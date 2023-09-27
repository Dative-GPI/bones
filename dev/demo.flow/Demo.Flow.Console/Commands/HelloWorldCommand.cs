using Bones.Flow;
using Demo.Flow.Console.Abstractions;

namespace Demo.Flow.Console.Commands
{
    public class HelloWorldCommand : IRequest, IWriteLineRequest
    {
        public string Message { get; set; } = "Hello world";

        public string Test => throw new System.NotImplementedException();
    }
}