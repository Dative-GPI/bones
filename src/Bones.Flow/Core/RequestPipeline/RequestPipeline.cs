using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


using Bones.Monitoring;
using static Bones.Flow.Core.Consts;
using static Bones.Flow.MetricExtensions;

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
        IHistogram<double> _pipelineHistogram;
        IHistogram<double> _middlewareHistogram;
        ILogger<RequestPipeline<TRequest>> _logger;

        IUnitOfWork _unitOfWork;
        bool _configured;

        public RequestPipeline(ILogger<RequestPipeline<TRequest>> logger,
            IMetricFactory metricFactory,
            ITraceFactory traceFactory
        )
        {
            _traceFactory = traceFactory;
            _pipelineHistogram = metricFactory.GetHistogram<double>(BONES_FLOW_PIPELINE_HISTOGRAM, METER, "ms", "Pipeline duration");
            _middlewareHistogram = metricFactory.GetHistogram<double>(BONES_FLOW_MIDDLEWARE_HISTOGRAM, METER, "ms", "Middleware duration");
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

            await ExecutePipeline(request, cancellationToken, commit: commit);
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

        public async Task ExecutePipeline(TRequest request, CancellationToken cancellationToken, Func<Task> next = default, bool commit = false)
        {
            using (_pipelineTrace = _traceFactory.CreatePipelineTrace<TRequest>())
            {
                var timer = Stopwatch.StartNew();
                try
                {
                    await ExecuteMiddlewares(request, cancellationToken, next);
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

                await ExecuteSuccessHandlers(request, cancellationToken);

                if(commit && _successHandlers.Any())
                {
                    await Commit();
                }

                timer.Stop();
                _pipelineHistogram.Record<TRequest>(timer.ElapsedMilliseconds);
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

            var trace = _traceFactory.CreateMiddlewareTrace<TRequest>(middleware, _pipelineTrace);
            var timer = Stopwatch.StartNew();

            try
            {
                await middleware.HandleAsync(
                    request,
                    async () =>
                    {
                        await ExecuteMiddleware(request, index + 1, cancellationToken, next);
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
                    ex.Data[LOGGED] = true;
                    trace.SetError(ex, request);
                }
                trace.Dispose();
                throw;
            }

            timer.Stop();
            trace.Dispose();
            _middlewareHistogram.Record(timer.ElapsedMilliseconds, middleware.GetType().ToColloquialString());
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