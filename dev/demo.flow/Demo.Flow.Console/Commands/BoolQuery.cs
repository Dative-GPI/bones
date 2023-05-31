using Bones.Flow;
using Demo.Flow.Console.Abstractions;

namespace Demo.Flow.Console.Commands
{
    public class BoolQuery : IRequest<bool>, IBoolRequest
    {
        public string Message { get; set; }
    }
}