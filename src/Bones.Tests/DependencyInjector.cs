using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Bones.Tests.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddDebug(this IServiceCollection services, ITestOutputHelper output)
        {
            services.AddSingleton<ITestOutputHelper>(output);
            
            services.AddScoped(typeof(ILogger<>), typeof(XunitLogger<>));

            return services;
        }
    }
}