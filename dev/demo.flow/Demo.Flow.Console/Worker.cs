using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading;
using System;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Flow.Console.Commands;
using System.Diagnostics;

namespace Demo.Flow.Console
{
    public class Worker : IHostedService
    {
        private IServiceProvider _services;

        public Worker(IServiceProvider services)
        {
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {            
                var logger = _services.GetRequiredService<ILogger<Program>>();

                Activity.CurrentChanged += (sender, args) =>
                {
                    var current = Activity.Current;
                    if (current != null)
                    {
                        logger.LogInformation($"Activity.CurrentChanged: {current.OperationName}");
                    }
                };

                ICommandHandler<HelloWorldCommand> helloCommandHandler = _services.GetRequiredService<ICommandHandler<HelloWorldCommand>>();
                IQueryHandler<BoolQuery, bool> boolQueryHandler = _services.GetRequiredService<IQueryHandler<BoolQuery, bool>>();

                var command = new HelloWorldCommand();
                command.Message = "First Command";

                var query = new BoolQuery();
                query.Message = "First Query";

                // while(true)
                // {
                    await helloCommandHandler.HandleAsync(command);
                    // var res = await boolQueryHandler.HandleAsync(query);
                    // System.Console.WriteLine(res);
                    await Task.Delay(1000, cancellationToken);
                // }

                // return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}