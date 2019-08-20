using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Chronos.Domain.Requests;
using Chronos.Domain.Pipelines.Interfaces;
using Chronos.Domain.Interfaces;

namespace Chronos.Domain.Pipelines
{
    public class Pipeline<TRequest> : IPipeline<TRequest>
    {
        private IEnumerable<IMiddleware<TRequest>> _middlewares;
        private IUnitOfWork _uow;
        private ILogger<Pipeline<TRequest>> _logger;
        private PipelineMode _mode;

        public Pipeline(ILogger<Pipeline<TRequest>> logger,
            IUnitOfWork uow,
            IEnumerable<IMiddleware<TRequest>> middlewares,
            PipelineMode mode)
        {
            _uow = uow;
            _logger = logger;
            _middlewares = middlewares;
            _mode = mode;
        }

        async Task<RequestResult> ICommandHandler<TRequest>.HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit)
        {
            var result = await this.Execute(request, cancellationToken);
            if (commit && result.Succeed)
            {
                await _uow.Commit();
            }
            return result;
        }

        async Task<RequestResult> IQueryHandler<TRequest>.HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var result = await this.Execute(request, cancellationToken);
            return result;
        }

        async Task<RequestResult> IMiddleware<TRequest>.HandleAsync(TRequest request, Func<Task<RequestResult>> next, CancellationToken cancellationToken)
        {
            var result = await this.Execute(request, cancellationToken);
            if (result.Succeed)
            {
                result = await next();
            }
            return result;
        }

        private async Task<RequestResult> Execute(TRequest request, CancellationToken cancellationToken)
        {
            RequestResult result;
            switch (_mode)
            {
                case PipelineMode.Or:
                    result = await OrExecute(request, cancellationToken, _middlewares);
                    break;
                case PipelineMode.And:
                    result = await AndExecute(request, cancellationToken, _middlewares);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return result;
        }

        private async Task<RequestResult> OrExecute(TRequest request, CancellationToken cancellationToken, IEnumerable<IMiddleware<TRequest>> middlewares)
        {
            RequestResult lastResult = RequestResult.Fail();

            foreach (var middleware in middlewares)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                lastResult = await middleware.HandleAsync(
                    request, () => Task.FromResult(RequestResult.Success()),
                    cancellationToken
                );

                if (lastResult.Succeed)
                {
                    break;
                }
            }

            return lastResult;
        }

        private async Task<RequestResult> AndExecute(TRequest request, CancellationToken cancellationToken, IEnumerable<IMiddleware<TRequest>> middlewares)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var middleware = middlewares.FirstOrDefault();
            if (middleware != default)
            {
                Func<Task<RequestResult>> next = null;

                if (middlewares.Skip(1).Any())
                {
                    next = () => this.AndExecute(request, cancellationToken, middlewares.Skip(1));
                }

                return await middleware.HandleAsync(request, next, cancellationToken);
            }
            else
            {
                throw new IndexOutOfRangeException("No more middlewares");
            }
        }
    }
}