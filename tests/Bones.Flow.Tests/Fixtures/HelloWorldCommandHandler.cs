using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class HelloWorldCommandHandler : IMiddleware<HelloWorldCommand>
    {
        public Task HandleAsync(HelloWorldCommand request, Func<Task> next, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Message);

            return Task.CompletedTask;
        }
    }
}