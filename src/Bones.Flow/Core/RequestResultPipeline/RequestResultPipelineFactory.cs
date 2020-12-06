using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow.Core
{
    internal class RequestResultPipelineFactory<TRequest, TResult> : IBuildablePipelineFactory<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        IServiceProvider _provider;
        List<IMiddleware<TRequest>> _requestMiddlewares;
        List<IMiddleware<TRequest, TResult>> _requestResultMiddlewares;
        List<ISuccessHandler<TRequest>> _requestSuccessHandler;
        List<ISuccessHandler<TRequest, TResult>> _requestResultSuccessHandler;
        List<IFailureHandler<TRequest>> _failureHandlers;
        List<MiddlewareType> _middlewareTypes;

        public RequestResultPipelineFactory(IServiceProvider provider)
        {
            _provider = provider;
            
            _requestMiddlewares = new List<IMiddleware<TRequest>>();
            _requestResultMiddlewares = new List<IMiddleware<TRequest, TResult>>();
            
            _requestSuccessHandler = new List<ISuccessHandler<TRequest>>();
            _requestResultSuccessHandler = new List<ISuccessHandler<TRequest, TResult>>();

            _failureHandlers = new List<IFailureHandler<TRequest>>();

            _middlewareTypes = new List<MiddlewareType>();
        }

        public IPipeline<TRequest, TResult> Build()
        {
            var pipeline = _provider.GetRequiredService<IPipeline<TRequest, TResult>>();

            pipeline.Configure(
                _middlewareTypes,
                _requestMiddlewares,
                _requestResultMiddlewares,
                _requestSuccessHandler,
                _requestResultSuccessHandler,
                _failureHandlers
            );
            
            return pipeline;
        }

        public IBuildablePipelineFactory<TRequest, TResult> Add<TMiddleware>() where TMiddleware : IMiddleware<TRequest, TResult>
        {
            var middleware = _provider.GetRequiredService<TMiddleware>();

            _requestResultMiddlewares.Add(middleware);
            _middlewareTypes.Add(MiddlewareType.RequestResultMiddleware);

            return this;
        }

        public IPipelineFactory<TRequest, TResult> With<TMiddleware>() where TMiddleware : IMiddleware<TRequest>
        {
            var middleware = _provider.GetRequiredService<TMiddleware>();

            _requestMiddlewares.Add(middleware);
            _middlewareTypes.Add(MiddlewareType.RequestMiddleware);

            return this;
        }

        public IBuildablePipelineFactory<TRequest, TResult> OnFailure<THandler>() where THandler : IFailureHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();

            _failureHandlers.Add(handler);

            return this;
        }

        public IBuildablePipelineFactory<TRequest, TResult> OnSuccess<THandler>() where THandler : ISuccessHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();

            _requestSuccessHandler.Add(handler);

            return this;
        }

        public IBuildablePipelineFactory<TRequest, TResult> OnResult<THandler>() where THandler : ISuccessHandler<TRequest, TResult>
        {
            var handler = _provider.GetRequiredService<THandler>();

            _requestResultSuccessHandler.Add(handler);

            return this;
        }
    }
}