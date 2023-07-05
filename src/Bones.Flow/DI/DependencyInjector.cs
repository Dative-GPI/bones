using System;
using System.Diagnostics.Metrics;
using Bones.Flow.Core;
using Bones.Monitoring;
using Bones.Monitoring.Core;
using Microsoft.Extensions.DependencyInjection;

using static Bones.Flow.Core.Consts;

namespace Bones.Flow
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddFlow(this IServiceCollection services, Action<BonesMonitoringOptions> configureMonitoringOptions = null)
        {
            services.AddScoped(typeof(IPipelineFactory<>), typeof(RequestPipelineFactory<>));
            services.AddScoped(typeof(IPipeline<>), typeof(RequestPipeline<>));

            services.AddScoped(typeof(IPipelineFactory<,>), typeof(RequestResultPipelineFactory<,>));
            services.AddScoped(typeof(IPipeline<,>), typeof(RequestResultPipeline<,>));

            services.AddMonitoring(BONES_FLOW_INSTRUMENTATION, configureMonitoringOptions);
            
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