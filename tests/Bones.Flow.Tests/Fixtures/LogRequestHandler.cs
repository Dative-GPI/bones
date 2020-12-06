

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class LogRequestHandler<TRequest, TResult> : ISuccessHandler<TRequest, TResult> where TRequest : IRequest<TResult>

    {
        public Task HandleAsync(TRequest request, TResult result, CancellationToken cancellationToken)
        {
            Console.WriteLine(request);

            return Task.CompletedTask;
        }
    }

    public class LogRequestHandler<TRequest> : ISuccessHandler<TRequest> where TRequest: IRequest
    {
        public Task HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request);

            return Task.CompletedTask;
        }
    }
}