using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Bones.Requests.Pipelines.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bones.Requests.Pipelines
{
    public class PipelineFactory : IPipelineFactory
    {
        private IServiceProvider _sp;

        public PipelineFactory(IServiceProvider sp)
        {
            _sp = sp;
        }
        public IPipelineFactory<T> Create<T>(PipelineMode mode = PipelineMode.And)
        {
            return new PipelineFactory<T>(_sp, mode);
        }
    }

    public class PipelineFactory<TRequest> : IPipelineFactory<TRequest>
    {
        List<IMiddleware<TRequest>> _middlewares;
        PipelineMode _mode;
        IServiceProvider _provider;

        public PipelineFactory(IServiceProvider provider, PipelineMode mode)
        {
            _middlewares = new List<IMiddleware<TRequest>>();
            _provider = provider;
            _mode = mode;
        }

        public IPipelineFactory<TRequest> Add<TMiddleware>() where TMiddleware : IMiddleware<TRequest>
        {
            var middleware = _provider.GetService<TMiddleware>();
            if (middleware == default)
            {
                throw new NullReferenceException($"Service {typeof(TMiddleware)} has not been added to DI");
            }
            _middlewares.Add(middleware);
            return this;
        }
        public IPipelineFactory<TRequest> Add(IMiddleware<TRequest> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }


        public IPipeline<TRequest> Finally(IMiddleware<TRequest> middleware)
        {
            Add(middleware);
            return Build();
        }

        public IPipeline<TRequest> Finally<TMiddleware>() where TMiddleware : IMiddleware<TRequest>
        {
            Add<TMiddleware>();
            return Build();
        }

        public IPipeline<TRequest> Build()
        {
            var logger = _provider.GetService<ILogger<Pipeline<TRequest>>>();
            var uow = _provider.GetService<IUnitOfWork>();
            return new Pipeline<TRequest>(logger, uow, _middlewares, _mode);
        }
    }
}