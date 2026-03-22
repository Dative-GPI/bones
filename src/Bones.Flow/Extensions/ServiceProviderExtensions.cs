using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow
{
    public static class ServiceProviderExtensions
    {
        public static IPipelineFactory<TRequest> GetPipelineFactory<TRequest>(this IServiceProvider provider) where TRequest: IRequest
        {
            return provider.GetRequiredService<IPipelineFactory<TRequest>>();
        }

        public static IPipelineFactory<TRequest, TResult> GetPipelineFactory<TRequest, TResult>(this IServiceProvider provider) where TRequest: IRequest<TResult>
        {
            return provider.GetRequiredService<IPipelineFactory<TRequest, TResult>>();
        }

        public static IResponsibilityChainFactory<TRequest> GetResponsibilityChainFactory<TRequest>(this IServiceProvider provider) where TRequest : IRequest
        {
            return provider.GetRequiredService<IResponsibilityChainFactory<TRequest>>();
        }

        internal static IResponsibilityChainLink<TRequest> GetResponsibilityChainLink<TRequest>(
            this IServiceProvider provider,
            IResponsibilityChainHandler<TRequest> handler
        ) where TRequest : IRequest
        {
            var wrapper = provider.GetRequiredService<IResponsibilityChainLink<TRequest>>();
            wrapper.SetHandler(handler);
            return wrapper;
        }
    }
}