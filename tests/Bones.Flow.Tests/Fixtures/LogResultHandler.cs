using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class LogResultHandler<TResult> : ISuccessHandler<IRequest<TResult>, TResult>
    {
        public Task HandleAsync(IRequest<TResult> request, TResult result, CancellationToken cancellationToken)
        {
            Console.WriteLine(result);

            return Task.CompletedTask;
        }
    }
}