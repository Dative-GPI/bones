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

        // public static IPipeline<TRequest> CreateSimpleHandler<TRequest, THandler>(this IServiceProvider provider)
        //     where THandler : IMiddleware<TRequest>
        // {
        //     return provider.GetRequiredService<IPipeline<TRequest>>()
        //         .Add<THandler>();
        // }
    }
}