using System;

using Microsoft.Extensions.DependencyInjection;

using Bones.Requests.Pipelines.Interfaces;
using Bones.Requests;

public static class ServiceProviderExtensions
{
    public static IPipelineFactory<TRequest> GetPipelineFactory<TRequest>(this IServiceProvider provider)
    {
        return provider.GetService<IPipelineFactory>()
            .Create<TRequest>();
    }

    public static IQueryHandler<TRequest> FromQueryHandler<THandler, TRequest>(this IServiceProvider provider)
        where THandler : IMiddleware<TRequest>
    {
        return provider.GetPipelineFactory<TRequest>()
            .Finally<THandler>();
    }

    public static ICommandHandler<TRequest> FromCommandHandler<THandler, TRequest>(this IServiceProvider provider)
        where THandler : IMiddleware<TRequest>
    {
        return provider.GetPipelineFactory<TRequest>()
            .Finally<THandler>();
    }
}