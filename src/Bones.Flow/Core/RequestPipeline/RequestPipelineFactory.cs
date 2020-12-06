using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow.Core
{
    internal class RequestPipelineFactory<TRequest> : IPipelineFactory<TRequest> where TRequest : IRequest
    {
        List<IMiddleware<TRequest>> _middlewares;
        List<ISuccessHandler<TRequest>> _successHandlers;
        List<IFailureHandler<TRequest>> _failureHandlers;
        IServiceProvider _provider;

        public RequestPipelineFactory(IServiceProvider provider)
        {
            _provider = provider;

            _middlewares = new List<IMiddleware<TRequest>>();
            _successHandlers = new List<ISuccessHandler<TRequest>>();
            _failureHandlers = new List<IFailureHandler<TRequest>>();
        }


        public IPipelineFactory<TRequest> Add<TMiddleware>() where TMiddleware : IMiddleware<TRequest>
        {
            var middleware = _provider.GetRequiredService<TMiddleware>();

            return Add(middleware);
        }
        
        public IPipelineFactory<TRequest> Add<TMiddleware>(TMiddleware middleware) where TMiddleware : IMiddleware<TRequest>
        {
            _middlewares.Add(middleware);

            return this;
        }
        
        public IPipelineFactory<TRequest> OnFailure<THandler>() where THandler : IFailureHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();

            return OnFailure(handler);
        }

        public IPipelineFactory<TRequest> OnFailure<THandler>(THandler handler) where THandler : IFailureHandler<TRequest>
        {
            _failureHandlers.Add(handler);

            return this;
        }

        public IPipelineFactory<TRequest> OnSuccess<THandler>() where THandler : ISuccessHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();

            return OnSuccess(handler);
        }

        public IPipelineFactory<TRequest> OnSuccess<THandler>(THandler handler) where THandler : ISuccessHandler<TRequest>
        {
            _successHandlers.Add(handler);

            return this;
        }

        public IPipeline<TRequest> Build()
        {
            var pipeline = _provider.GetRequiredService<IPipeline<TRequest>>();
            var uow = _provider.GetService<IUnitOfWork>();

            pipeline.Configure(
                _middlewares,
                _successHandlers,
                _failureHandlers,
                uow
            );

            return pipeline;
        }
    }
}