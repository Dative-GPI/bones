using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bones.Flow.Core
{
    internal class ResultChainOfResponsibility<TRequest, TResult> : IChainOfResponsibility<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private List<IChainOfResponsibilityHandler<TRequest, TResult>> _handlers = new List<IChainOfResponsibilityHandler<TRequest, TResult>>();
        private ILogger<ResultChainOfResponsibility<TRequest, TResult>> _logger;

        public ResultChainOfResponsibility(
            ILogger<ResultChainOfResponsibility<TRequest, TResult>> logger
        )
        {
            _logger = logger;
        }

        public void Configure(List<IChainOfResponsibilityHandler<TRequest, TResult>> handlers)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        async Task<TResult> ICommandHandler<TRequest, TResult>.HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit)
        {
            foreach (var handler in _handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var (handled, result) = await handler.HandleAsync(request, cancellationToken);
                    if (handled)
                        return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in ChainOfResponsibility handler {handler}", handler.GetType().Name);
                    throw;
                }
            }

            return default;
        }

        async Task<TResult> IMiddleware<TRequest, TResult>.HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken)
        {
            foreach (var handler in _handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var (handled, result) = await handler.HandleAsync(request, cancellationToken);
                    if (handled)
                        return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in ChainOfResponsibility handler {handler}", handler.GetType().Name);
                    throw;
                }
            }

            return await next();
        }
    }
}
