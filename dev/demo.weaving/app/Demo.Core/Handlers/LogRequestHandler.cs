

using System;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;

namespace Demo.Core.Handlers
{
    public class LogRequestHandler<TRequest, TResult> : ISuccessHandler<TRequest, TResult> where TRequest : IRequest<TResult>

    {
        public Task HandleAsync(TRequest request, TResult result, CancellationToken cancellationToken)
        {
            System.Console.WriteLine(request);

            return Task.CompletedTask;
        }
    }

    public class LogRequestHandler<TRequest> : ISuccessHandler<TRequest> where TRequest: IRequest
    {
        public Task HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            System.Console.WriteLine(request);

            return Task.CompletedTask;
        }
    }
}