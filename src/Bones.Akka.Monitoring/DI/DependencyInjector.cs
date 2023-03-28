using System;
using Bones.Monitoring;
using Bones.Monitoring.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Akka.Monitoring.DI
{
    public static class DependencyInjector
    {

        public static IServiceCollection AddAkkaMonitoring(this IServiceCollection services, Action<BonesMonitoringOptions> configureMonitoringOptions = null)
        {
            services.AddSingleton<AkkaMonitor>();
            services.AddMonitoring(Consts.BONES_AKKA_MONITORING_INSTRUMENTATION, configureMonitoringOptions);

            return services;
        }
    }
}