
using Bones.Akka.DI;
using Bones.Flow;
using Demo.Core.Actors;
using Demo.Core.Commands;
using Demo.Core.Handlers;
using Demo.Core.Middlewares;
using Demo.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddAkka("BonesAkkaMonitoringConsole");
            services.AddRootCreator<RootActor>();
            services.AddCreator<IPingActor, PingActor>();
            services.AddCreator<IPongActor, PongActor>();

            services.AddScoped<ActorLicitMiddleware>();
            services.AddScoped<LogRequestHandler<HelloWorldCommand>>();
            services.AddScoped<LogExceptionHandler<HelloWorldCommand>>();
            services.AddScoped<HelloWorldCommandHandler>();
            
            services.AddScoped<ICommandHandler<HelloWorldCommand>>(sp =>
            {
                var pipeline = sp.GetPipelineFactory<HelloWorldCommand>()
                    .Add<ActorLicitMiddleware>()
                    .Add<HelloWorldCommandHandler>()
                    .OnSuccess<LogRequestHandler<HelloWorldCommand>>()
                    .OnFailure<LogExceptionHandler<HelloWorldCommand>>()
                    .Build();

                return pipeline;
            });


            return services;
        }
    }
}