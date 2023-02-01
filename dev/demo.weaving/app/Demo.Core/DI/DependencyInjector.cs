
using Bones.Akka.DI;
using Demo.Core.Actors;
using Demo.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddRootCreator<RootActor>();
            services.AddCreator<IPingActor, PingActor>();
            services.AddCreator<IPongActor, PongActor>();

            return services;
        }
    }
}