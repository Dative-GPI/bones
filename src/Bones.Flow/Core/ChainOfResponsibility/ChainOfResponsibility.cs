using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bones.Flow.Core
{
    internal class ChainOfResponsibility<TRequest> : IChainOfResponsibility<TRequest>
        where TRequest : IRequest
    {
        private List<IChainOfResponsibilityHandler<TRequest>> _handlers;
        private ILogger<ChainOfResponsibility<TRequest>> _logger;

        public ChainOfResponsibility(
            ILogger<ChainOfResponsibility<TRequest>> logger
        )
        {
            _logger = logger;
        }

        public void Configure(List<IChainOfResponsibilityHandler<TRequest>> handlers)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        async Task ICommandHandler<TRequest>.HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit)
        {
            foreach (var handler in _handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await handler.HandleAsync(request, cancellationToken))
                        return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in ChainOfResponsibility handler {handler}", handler.GetType().Name);
                    throw;
                }
            }
        }

        async Task IMiddleware<TRequest>.HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        {
            foreach (var handler in _handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await handler.HandleAsync(request, cancellationToken))
                        break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in ChainOfResponsibility handler {handler}", handler.GetType().Name);
                    throw;
                }
            }

            await next();
        }
    }
}
