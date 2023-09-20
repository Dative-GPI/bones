using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bones.Monitoring;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

using static Bones.Flow.Core.Consts;
using static Bones.Flow.MetricExtensions;
using System.Text.Json;

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
        IHistogram<double> _pipelineHistogram;
        IHistogram<double> _middlewareHistogram;
        ILogger<RequestResultPipeline<TRequest, TResult>> _logger;

        IUnitOfWork _unitOfWork;
        bool _configured;

        public RequestResultPipeline(
            ILogger<RequestResultPipeline<TRequest, TResult>> logger,
            ITraceFactory traceFactory,
            IMetricFactory metricFactory
        )
        {
            _traceFactory = traceFactory;
            _pipelineHistogram = metricFactory.GetHistogram<double>(BONES_FLOW_PIPELINE_HISTOGRAM, METER, "ms", "Pipeline duration");
            _middlewareHistogram = metricFactory.GetHistogram<double>(BONES_FLOW_MIDDLEWARE_HISTOGRAM, METER, "ms", "Middleware duration");
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

            var result = await ExecutePipeline(request, cancellationToken, commit: commit);

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
                    try
                    {
                        await _unitOfWork.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "An error occured when committing the unit of work");
                        trace.SetError(ex);
                        trace.Dispose();
                        throw;
                    }
                }
            }
        }


        private async Task<TResult> ExecutePipeline(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next = default, bool commit = false)
        {
            TResult result = default(TResult);

            using (_pipelineTrace = _traceFactory.CreatePipelineTrace<TRequest, TResult>())
            {
                var timer = Stopwatch.StartNew();
                try
                {
                    result = await ExecuteMiddlewares(request, cancellationToken, next);
                }
                catch (System.Exception ex)
                {
                    if (ex.Data.Contains(LOGGED))
                    {
                        ex.Data[LOGGED] = true;
                    }
                    _pipelineTrace.Dispose();
                    _logger.LogError(ex, "An error occured executing Pipeline<{TRequest}>", typeof(TRequest).Name);
                    await ExecuteFailureHandlers(request, ex, cancellationToken);
                    throw;
                }

                if (commit)
                {
                    await Commit();
                }

                await ExecuteSuccessHandlers(request, result, cancellationToken);

                if (commit && (_requestSuccessHandlers.Any() || _requestResultSuccessHandlers.Any()))
                {
                    await Commit();
                }

                timer.Stop();
                _pipelineHistogram.Record<TRequest>(timer.ElapsedMilliseconds);
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
            var timer = Stopwatch.StartNew();

            try
            {
                await middleware.HandleAsync(
                    request,
                    async () =>
                    {
                        result = await ExecuteMiddleware(request, counter, cancellationToken, next);
                    },
                    cancellationToken
                );

            }
            catch (System.Exception ex)
            {
                if (!ex.Data.Contains(LOGGED))
                {
                    _logger.LogError(ex, "An error occured in middleware {middleware} with payload {payload}",
                        middleware.GetType().Name,
                        JsonSerializer.Serialize(request));
                    trace.SetError(ex, request);
                    ex.Data[LOGGED] = true;
                }
                trace.Dispose();
                throw;
            }

            timer.Stop();
            trace.Dispose();
            _middlewareHistogram.Record(timer.ElapsedMilliseconds, middleware.GetType().ToColloquialString());

            return result;
        }

        private async Task<TResult> ExecuteRequestResultMiddleware(TRequest request, Counter counter, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            var middleware = _requestResultMiddlewares[counter.RequestResultIndex];
            counter.RequestResultIndex++;

            TResult result = default(TResult);

            var trace = _traceFactory.CreateMiddlewareTrace(middleware, _pipelineTrace);
            var timer = Stopwatch.StartNew();

            try
            {
                result = await middleware.HandleAsync(
                    request,
                    async () =>
                    {
                        var tmp = await ExecuteMiddleware(request, counter, cancellationToken, next);
                        return tmp;
                    },
                    cancellationToken
                );

            }
            catch (System.Exception ex)
            {
                if (!ex.Data.Contains(LOGGED))
                {
                    _logger.LogError(ex, "An error occured in middleware {middleware} with payload {payload}",
                        middleware.GetType().Name,
                        JsonSerializer.Serialize(request));
                    trace.SetError(ex, request);
                    ex.Data[LOGGED] = true;
                }
                trace.Dispose();
                throw;
            }

            timer.Stop();
            trace.Dispose();
            _middlewareHistogram.Record(timer.ElapsedMilliseconds, middleware.GetType().ToColloquialString());

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