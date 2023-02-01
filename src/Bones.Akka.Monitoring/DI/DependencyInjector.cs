using Bones.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Akka.Monitoring.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddAkkaMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<AkkaMonitor>();
            services.AddMonitoring();

            return services;
        }
    }
}