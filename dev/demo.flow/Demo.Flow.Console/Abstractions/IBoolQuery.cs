using Bones.Flow;

namespace Demo.Flow.Console.Abstractions
{
    public interface IBoolRequest : IRequest
    {
        string Message { get; }
    }
}