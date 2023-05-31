using System;
using Bones.Monitoring.Core;
using Bones.Monitoring.Core.Metrics;
using Bones.Monitoring.Core.Sockets;
using Bones.Monitoring.Core.Tracing;
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

            services.AddSingleton<IMetricFactory, MetricFactory>();

            #region Sockets
            services.AddTelemetryConsumer<SocketsEventsConsumer>();
            services.AddMetricsConsumer<SocketsMetricsConsumer>();
            #endregion

            return services;
        }
    }
}