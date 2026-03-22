using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow.Core
{
    internal class ResponsibilityChainFactory<TRequest> : IResponsibilityChainFactory<TRequest> where TRequest : IRequest
    {
        List<IResponsibilityChainHandler<TRequest>> _handlers;
        IServiceProvider _provider;

        public ResponsibilityChainFactory(IServiceProvider provider)
        {
            _provider = provider;
            _handlers = new List<IResponsibilityChainHandler<TRequest>>();
        }


        public IResponsibilityChainFactory<TRequest> Add<THandler>() where THandler : IResponsibilityChainHandler<TRequest>
        {
            var handler = _provider.GetRequiredService<THandler>();
            return Add(handler);
        }

        public IResponsibilityChainFactory<TRequest> Add<THandler>(THandler handler) where THandler : IResponsibilityChainHandler<TRequest>
        {
            _handlers.Add(handler);
            return this;
        }

        public IResponsibilityChain<TRequest> Build()
        {
            var chain = _provider.GetRequiredService<IResponsibilityChain<TRequest>>();

            if (_handlers.Count() > 0)
            {
                var previous  = _provider.GetResponsibilityChainLink(_handlers[0]);
                chain.SetFirst(previous);

                for (var i = 1; i < _handlers.Count(); i++)
                {
                    var wrapper = _provider.GetResponsibilityChainLink(_handlers[i]);
                    previous.SetNext(wrapper);
                    previous = wrapper;
                }
            }

            return chain;
        }
    }
}