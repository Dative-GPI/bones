using System;
using Bones.Monitoring.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Monitoring
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddMonitoring(this IServiceCollection services, string optionsName = null, Action<BonesMonitoringOptions> configureMonitoringOptions = null)
        {
            if(optionsName != null && configureMonitoringOptions != null)
            {
                services.Configure<BonesMonitoringOptions>(optionsName, configureMonitoringOptions);
            }
            
            services.AddSingleton<ITraceFactory, TraceFactory>();
            
            return services;
        }
    }
}