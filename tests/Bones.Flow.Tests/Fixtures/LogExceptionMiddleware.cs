using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class LogExceptionHandler<TRequest> : IFailureHandler<TRequest> where TRequest : IRequest
    {
        public Task HandleAsync(TRequest request, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);

            return Task.CompletedTask;
        }
    }
}