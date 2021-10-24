using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Akka.Actor;
using Akka.Configuration;
using Akka.DependencyInjection;

namespace Bones.Akka.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddAkka(this IServiceCollection services, string systemName)
        {
            services.AddSingleton<ActorSystem>(sp =>
            {
                var configuration = ConfigurationFactory.FromObject(
                    sp.GetService<IConfiguration>()
                        .GetSection("Akka"));

                var di = DependencyResolverSetup.Create(sp);

                var setup = BootstrapSetup.Create()
                    .WithConfig(configuration)
                    .And(di);

                var actorSystem = ActorSystem.Create(systemName, setup);

                return actorSystem;
            });

            services.AddScoped<Creator>(sp =>
            {
                return (type, context) =>  DependencyResolver.For(context.System).Props(type);
            });

            return services;
        }

        public static IServiceCollection AddCreator<TActor>(this IServiceCollection services)
            where TActor : ActorBase
        {
            services.AddScoped<TActor>();

            services.AddScoped<Creator<TActor>>(sp =>
            {
                return (context) => DependencyResolver.For(context.System).Props<TActor>();
            });

            return services;
        }

        
        public static IServiceCollection AddRootCreator<TActor>(this IServiceCollection services)
            where TActor : ActorBase
        {
            services.AddScoped<TActor>();

            services.AddSingleton<RootCreator<TActor>>(sp =>
            {
                return (context) => DependencyResolver.For(context).Props<TActor>();
            });

            return services;
        }

        public static IServiceCollection AddCreator<TInterface, TActor>(this IServiceCollection services)
            where TActor : ActorBase, TInterface
        {
            services.AddScoped<TActor>();

            services.AddScoped<Creator<TInterface>>(sp =>
            {
                return (context) => DependencyResolver.For(context.System).Props<TActor>();
            });

            return services;
        }

        public static IServiceCollection AddActorRef<TInterface>(this IServiceCollection services, string pattern)
        {
            services.AddSingleton<IActorRefProvider<TInterface>>(sp => {
                var actorSystem = sp.GetRequiredService<ActorSystem>();
                return new ActorRefProvider<TInterface>(
                    actorSystem.ActorSelection(pattern)
                );
            });

            return services;
        }
    }
}