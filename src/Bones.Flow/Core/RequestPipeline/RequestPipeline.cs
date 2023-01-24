using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Bones.Monitoring;
using Microsoft.Extensions.Logging;

using static Bones.Flow.Core.Consts;

namespace Bones.Flow.Core
{
    internal class RequestPipeline<TRequest> : IPipeline<TRequest>
        where TRequest : IRequest
    {
        List<IMiddleware<TRequest>> _middlewares;
        List<ISuccessHandler<TRequest>> _successHandlers;
        List<IFailureHandler<TRequest>> _failureHandlers;

        ITrace _pipelineTrace;
        ITraceFactory _traceFactory;
        ILogger<RequestPipeline<TRequest>> _logger;

        IUnitOfWork _unitOfWork;
        bool _configured;

        public RequestPipeline(ILogger<RequestPipeline<TRequest>> logger,
            ITraceFactory traceFactory
        )
        {
            _traceFactory = traceFactory;
            _logger = logger;
        }

        public void Configure(IEnumerable<IMiddleware<TRequest>> middlewares,
            IEnumerable<ISuccessHandler<TRequest>> successHandlers,
            IEnumerable<IFailureHandler<TRequest>> failureHandlers,
            IUnitOfWork unitOfWork = null)
        {
            Contract.Assert(middlewares.Any(), "Empty pipeline");
            Contract.Assert(successHandlers != null);
            Contract.Assert(failureHandlers != null);

            _successHandlers = successHandlers.ToList();
            _failureHandlers = failureHandlers.ToList();
            _middlewares = middlewares.ToList();
            _unitOfWork = unitOfWork;

            _configured = true;
        }

        async Task ICommandHandler<TRequest>.HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit)
        {
            Contract.Assert(_configured, "Pipeline not configured");

            await ExecutePipeline(request, cancellationToken,
                beforeSuccess: commit ? Commit : (Func<Task>)null
            );

            if (commit)
            {
                await Commit();
            }
        }

        async Task IMiddleware<TRequest>.HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        {
            Contract.Assert(_configured, "Pipeline not configured");

            await ExecutePipeline(request, cancellationToken, next);
        }


        private async Task Commit()
        {
            if (_unitOfWork != null)
            {
                using (var trace = _traceFactory.CreateCommitTrace(_pipelineTrace))
                {
                    await _unitOfWork.Commit();
                }
            }
        }

        public async Task ExecutePipeline(TRequest request, CancellationToken cancellationToken, Func<Task> next = default, Func<Task> beforeSuccess = null)
        {
            using (_pipelineTrace = _traceFactory.CreatePipelineTrace<TRequest>())
            {
                try
                {
                    await ExecuteMiddlewares(request, cancellationToken, next);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "An error occured executing Pipeline<{TRequest}>", typeof(TRequest).Name);
                    await ExecuteFailureHandlers(request, ex, cancellationToken);

                    throw;
                }

                if (beforeSuccess != null)
                {
                    await beforeSuccess();
                }


                await ExecuteSuccessHandlers(request, cancellationToken);
            }
        }

        private Task ExecuteMiddlewares(TRequest request, CancellationToken cancellationToken, Func<Task> next)
        {
            return ExecuteMiddleware(request, 0, cancellationToken, next);
        }

        private async Task ExecuteMiddleware(TRequest request, int index, CancellationToken cancellationToken, Func<Task> next)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (index == _middlewares.Count)
            {
                if (next != default) await next();
                return;
            }

            var middleware = _middlewares[index];

            using (var trace = _traceFactory.CreateMiddlewareTrace<TRequest>(middleware, _pipelineTrace))
            {
                try
                {
                    await middleware.HandleAsync(
                        request,
                        async () =>
                        {
                            trace.Stop();
                            await ExecuteMiddleware(request, index + 1, cancellationToken, next);
                            trace.Start();
                        },
                        cancellationToken
                    );
                }
                catch (System.Exception ex) when (ex.Data[TRACED] is bool traced && !traced)
                {
                    _logger.LogError(ex, "An error occured in middleware {middleware}", middleware.GetType().Name);
                    ex.Data[TRACED] = true;
                    throw ex;
                }
            }
        }

        private async Task ExecuteFailureHandlers(TRequest request, Exception source, CancellationToken cancellationToken)
        {
            foreach (var handler in _failureHandlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteFailureHandler(request, handler, source, cancellationToken);
            }
        }

        private async Task ExecuteFailureHandler(TRequest request, IFailureHandler<TRequest> handler, Exception source, CancellationToken cancellationToken)
        {
            try
            {
                using (var trace = _traceFactory.CreateFailureHandlerTrace<TRequest>(handler, _pipelineTrace))
                {
                    await handler.HandleAsync(request, source, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occured when executing failure handler {handler}", handler.GetType().Name);
            }
        }

        private async Task ExecuteSuccessHandlers(TRequest request, CancellationToken cancellationToken)
        {
            foreach (var handler in _successHandlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteSuccessHandler(handler, request, cancellationToken);
            }
        }

        private async Task ExecuteSuccessHandler(ISuccessHandler<TRequest> handler, TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using (var trace = _traceFactory.CreateSuccessHandlerTrace<TRequest>(handler, _pipelineTrace))
                {
                    await handler.HandleAsync(request, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occured when executing failure handler {handler}", handler.GetType().Name);
            }
        }
    }
}