using System;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;

namespace Demo.Akka.Monitoring.Console.Handlers
{
    public class LogExceptionHandler<TRequest> : IFailureHandler<TRequest> where TRequest : IRequest
    {
        public Task HandleAsync(TRequest request, Exception exception, CancellationToken cancellationToken)
        {
            System.Console.WriteLine(exception.Message);

            return Task.CompletedTask;
        }
    }
}