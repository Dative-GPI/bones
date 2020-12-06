using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class ConcatCommandHandler : IMiddleware<ConcatCommand, string>
    {
        public Task<string> HandleAsync(ConcatCommand request, Func<Task<string>> next, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Left + request.Right);
        }
    }
}