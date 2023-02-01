using Bones.Flow.Core;
using Bones.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Flow
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddFlow(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPipelineFactory<>), typeof(RequestPipelineFactory<>));
            services.AddScoped(typeof(IPipeline<>), typeof(RequestPipeline<>));

            services.AddScoped(typeof(IPipelineFactory<,>), typeof(RequestResultPipelineFactory<,>));
            services.AddScoped(typeof(IPipeline<,>), typeof(RequestResultPipeline<,>));

            services.AddMonitoring();
            
            return services;
        }

        public static IServiceCollection AddFlow<TUnitOfWork>(this IServiceCollection services)
            where TUnitOfWork : class, IUnitOfWork
        {
            services.AddFlow();

            services.AddScoped<IUnitOfWork, TUnitOfWork>();

            return services;
        }
    }
}