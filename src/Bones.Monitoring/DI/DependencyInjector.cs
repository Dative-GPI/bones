using Bones.Monitoring;
using Bones.Monitoring.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Monitoring
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddMonitoring(this IServiceCollection services)
        {

            services.AddSingleton<ITraceFactory, TraceFactory>();
            
            return services;
        }
    }
}