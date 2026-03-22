using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow.Core
{
    internal class ResultChainOfResponsibilityFactory<TRequest, TResult> : IChainOfResponsibilityFactory<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private IServiceProvider _provider;
        private List<IChainOfResponsibilityHandler<TRequest, TResult>> _handlers;

        public ResultChainOfResponsibilityFactory(IServiceProvider provider)
        {
            _provider = provider;
            _handlers = new List<IChainOfResponsibilityHandler<TRequest, TResult>>();
        }

        public IChainOfResponsibilityFactory<TRequest, TResult> Add<THandler>()
            where THandler : IChainOfResponsibilityHandler<TRequest, TResult>
        {
            var handler = _provider.GetRequiredService<THandler>();
            return Add(handler);
        }

        public IChainOfResponsibilityFactory<TRequest, TResult> Add<THandler>(THandler handler)
            where THandler : IChainOfResponsibilityHandler<TRequest, TResult>
        {
            _handlers.Add(handler);
            return this;
        }

        public IChainOfResponsibility<TRequest, TResult> Build()
        {
            var chain = _provider.GetRequiredService<IChainOfResponsibility<TRequest, TResult>>();
            chain.Configure(_handlers);
            return chain;
        }
    }
}
