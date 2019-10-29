

using Bones.Requests.Pipelines;
using Bones.Requests.Pipelines.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.DI
{
    public static class Injector
    {
        public static IServiceCollection AddBones(this IServiceCollection services)
        {
            services.AddScoped<IPipelineFactory, PipelineFactory>();
            
            return services;
        }
    }
}