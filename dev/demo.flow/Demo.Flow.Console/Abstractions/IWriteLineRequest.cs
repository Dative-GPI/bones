using Bones.Flow;

namespace Demo.Flow.Console.Abstractions
{
    public interface IWriteLineRequest : IRequest
    {
        string Message { get; }
    }
}