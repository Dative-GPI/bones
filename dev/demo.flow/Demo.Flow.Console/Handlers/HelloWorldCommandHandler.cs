using System;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Flow.Console.Commands;

namespace Demo.Flow.Console.Handlers
{
    public class HelloWorldCommandHandler : IMiddleware<HelloWorldCommand>
    {
        public async Task HandleAsync(HelloWorldCommand request, Func<Task> next, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("BEFORE" + request.Message);

            await next();

            System.Console.WriteLine("AFTER" + request.Message);
        }
    }
}