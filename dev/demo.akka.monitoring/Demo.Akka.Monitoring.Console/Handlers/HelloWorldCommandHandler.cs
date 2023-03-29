using System;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Akka.Monitoring.Console.Commands;

namespace Demo.Akka.Monitoring.Console.Handlers
{
    public class HelloWorldCommandHandler : IMiddleware<HelloWorldCommand>
    {
        public Task HandleAsync(HelloWorldCommand request, Func<Task> next, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("TEST" + request.Message);

            return Task.CompletedTask;
        }
    }
}