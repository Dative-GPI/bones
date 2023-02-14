using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bones.Monitoring;
using Microsoft.Extensions.Logging;

using static Bones.Flow.Core.Consts;

namespace Bones.Flow.Core
{
    internal class RequestResultPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        List<MiddlewareType> _middlewareTypes;
        List<IMiddleware<TRequest>> _requestMiddlewares;
        List<IMiddleware<TRequest, TResult>> _requestResultMiddlewares;

        List<ISuccessHandler<TRequest>> _requestSuccessHandlers;
        List<ISuccessHandler<TRequest, TResult>> _requestResultSuccessHandlers;
        List<IFailureHandler<TRequest>> _failureHandlers;

        ITrace _pipelineTrace;
        ITraceFactory _traceFactory;
        ILogger<RequestResultPipeline<TRequest, TResult>> _logger;

        IUnitOfWork _unitOfWork;
        bool _configured;

        public RequestResultPipeline(
            ILogger<RequestResultPipeline<TRequest, TResult>> logger,
            ITraceFactory traceFactory
        )
        {
            _traceFactory = traceFactory;
            _logger = logger;
        }

        public void Configure(List<MiddlewareType> middlewareTypes,
            List<IMiddleware<TRequest>> requestMiddlewares,
            List<IMiddleware<TRequest, TResult>> requestResultMiddlewares,
            List<ISuccessHandler<TRequest>> requestSuccessHandler,
            List<ISuccessHandler<TRequest, TResult>> requestResultSuccessHandler,
            List<IFailureHandler<TRequest>> failureHandlers,
            IUnitOfWork uow = null)
        {
            Contract.Assert(middlewareTypes.Any(), "Empty pipeline");
            Contract.Assert(requestMiddlewares != null);
            Contract.Assert(requestResultMiddlewares != null);

            Contract.Assert(requestSuccessHandler != null);
            Contract.Assert(requestResultSuccessHandler != null);

            Contract.Assert(failureHandlers != null);

            _middlewareTypes = middlewareTypes;
            _requestMiddlewares = requestMiddlewares;
            _requestResultMiddlewares = requestResultMiddlewares;

            _requestSuccessHandlers = requestSuccessHandler;
            _requestResultSuccessHandlers = requestResultSuccessHandler;

            _failureHandlers = failureHandlers;

            _unitOfWork = uow;
            _configured = true;
        }

        async Task<TResult> ICommandHandler<TRequest, TResult>.HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit)
        {
            Contract.Assert(_configured, "Pipeline not configured");

            var result = await ExecutePipeline(request, cancellationToken,
                beforeSuccess: commit ? Commit : (Func<Task>)null
            );

            if (commit)
            {
                await Commit();
            }

            return result;
        }

        async Task<TResult> IQueryHandler<TRequest, TResult>.HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            Contract.Assert(_configured, "Pipeline not configured");

            var result = await ExecutePipeline(request, cancellationToken);

            return result;
        }

        async Task<TResult> IMiddleware<TRequest, TResult>.HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken)
        {
            Contract.Assert(_configured, "Pipeline not configured");

            var result = await ExecutePipeline(request, cancellationToken, next);

            return result;
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


        private async Task<TResult> ExecutePipeline(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next = default, Func<Task> beforeSuccess = null)
        {
            TResult result = default(TResult);

            using (_pipelineTrace = _traceFactory.CreatePipelineTrace<TRequest, TResult>())
            {
                try
                {
                    result = await ExecuteMiddlewares(request, cancellationToken, next);
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

                await ExecuteSuccessHandlers(request, result, cancellationToken);
            }

            return result;
        }


        private Task<TResult> ExecuteMiddlewares(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            return ExecuteMiddleware(request, new Counter(), cancellationToken, next);
        }

        private async Task<TResult> ExecuteMiddleware(TRequest request, Counter counter, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (counter.Index == _middlewareTypes.Count)
            {
                if (next != default)
                {
                    return await next();
                }
                else
                {
                    // si aucun middleware n'a court-circuité la pipeline pour retourner un résultat
                    // jeter une exception ou default(TResult)
                    return default(TResult);
                }
            }

            TResult result = default(TResult);

            var middlewareType = _middlewareTypes[counter.Index];
            counter.Index++;

            switch (middlewareType)
            {
                case MiddlewareType.RequestMiddleware:
                    result = await ExecuteRequestMiddleware(request, counter, cancellationToken, next);
                    break;
                case MiddlewareType.RequestResultMiddleware:
                    result = await ExecuteRequestResultMiddleware(request, counter, cancellationToken, next);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        private async Task<TResult> ExecuteRequestMiddleware(TRequest request, Counter counter, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            var middleware = _requestMiddlewares[counter.RequestIndex];
            counter.RequestIndex++;

            TResult result = default(TResult);

            var trace = _traceFactory.CreateMiddlewareTrace(middleware, _pipelineTrace);

            try
            {
                await middleware.HandleAsync(
                    request,
                    async () =>
                    {
                        trace.Dispose();
                        result = await ExecuteMiddleware(request, counter, cancellationToken, next);
                        trace = _traceFactory.CreateMiddlewareTrace(middleware, _pipelineTrace, true);
                    },
                    cancellationToken
                );

            }
            catch (System.Exception ex) when (ex.Data[TRACED] is bool traced && !traced)
            {
                trace.Dispose();
                _logger.LogError(ex, "An error occured in middleware {middleware}", middleware.GetType().Name);
                ex.Data[TRACED] = true;
                throw ex;
            }
            trace.Dispose();



            return result;
        }

        private async Task<TResult> ExecuteRequestResultMiddleware(TRequest request, Counter counter, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            var middleware = _requestResultMiddlewares[counter.RequestResultIndex];
            counter.RequestResultIndex++;

            TResult result = default(TResult);

            var trace = _traceFactory.CreateMiddlewareTrace(middleware, _pipelineTrace);

            try
            {
                result = await middleware.HandleAsync(
                    request,
                    async () =>
                    {
                        trace.Dispose();
                        var tmp = await ExecuteMiddleware(request, counter, cancellationToken, next);
                        trace = _traceFactory.CreateMiddlewareTrace(middleware, _pipelineTrace, true);
                        return tmp;
                    },
                    cancellationToken
                );

            }
            catch (System.Exception ex) when (ex.Data[TRACED] is bool traced && !traced)
            {
                trace.Dispose();
                _logger.LogError(ex, "An error occured in middleware {middleware}", middleware.GetType().Name);
                ex.Data[TRACED] = true;
                throw ex;
            }
            trace.Dispose();


            return result;
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


        private async Task ExecuteSuccessHandlers(TRequest request, TResult result, CancellationToken cancellationToken)
        {
            foreach (var handler in _requestSuccessHandlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteRequestSuccessHandler(handler, request, cancellationToken);
            }

            foreach (var handler in _requestResultSuccessHandlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteRequestResultSuccessHandler(handler, request, result, cancellationToken);
            }
        }

        private async Task ExecuteRequestSuccessHandler(ISuccessHandler<TRequest> handler, TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using (var trace = _traceFactory.CreateSuccessHandlerTrace(handler, _pipelineTrace))
                {
                    await handler.HandleAsync(request, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occured when executing failure handler {handler}", handler.GetType().Name);
            }
        }

        private async Task ExecuteRequestResultSuccessHandler(ISuccessHandler<TRequest, TResult> handler, TRequest request, TResult result, CancellationToken cancellationToken)
        {
            try
            {
                using (var trace = _traceFactory.CreateSuccessHandlerTrace(handler, _pipelineTrace))
                {
                    await handler.HandleAsync(request, result, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occured when executing failure handler {handler}", handler.GetType().Name);
            }
        }

        private struct Counter
        {
            public int Index { get; set; }
            public int RequestIndex { get; set; }
            public int RequestResultIndex { get; set; }
        }
    }
}