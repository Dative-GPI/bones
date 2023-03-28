using System;

using Bones.Flow;
using Demo.Core.Commands;

namespace Demo.Core.Handlers
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