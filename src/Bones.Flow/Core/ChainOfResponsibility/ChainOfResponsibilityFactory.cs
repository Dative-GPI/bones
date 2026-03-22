using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow.Core
{
    internal class ChainOfResponsibilityFactory<TRequest> : IChainOfResponsibilityFactory<TRequest>
        where TRequest : IRequest
    {
        private IServiceProvider _provider;
        private List<IChainOfResponsibilityHandler<TRequest>> _handlers;

        public ChainOfResponsibilityFactory(IServiceProvider provider)
        {
            _provider = provider;
            _handlers = new List<IChainOfResponsibilityHandler<TRequest>>();
        }

        public IChainOfResponsibilityFactory<TRequest> Add<THandler>()
            where THandler : IChainOfResponsibilityHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();
            return Add(handler);
        }

        public IChainOfResponsibilityFactory<TRequest> Add<THandler>(THandler handler)
            where THandler : IChainOfResponsibilityHandler<TRequest>
        {
            _handlers.Add(handler);
            return this;
        }

        public IChainOfResponsibility<TRequest> Build()
        {
            var chain = _provider.GetRequiredService<IChainOfResponsibility<TRequest>>();
            chain.Configure(_handlers);
            return chain;
        }
    }
}
