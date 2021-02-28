using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Akka.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddAkka(this IServiceCollection services, string systemName)
        {
            services.AddSingleton<ActorSystem>(sp =>
            {
                var actorSystem = ActorSystem.Create(systemName,
                    ConfigurationFactory.FromObject(
                        sp.GetService<IConfiguration>()
                            .GetSection("Akka")));

                actorSystem.AddDependencyResolver(
                    new MicrosoftDependencyResolver(
                        sp.GetService<IServiceScopeFactory>(),
                        actorSystem));

                return actorSystem;
            });

            services.AddScoped<Creator>(sp =>
            {
                return (type, context) => context.DI().Props(type);
            });

            return services;
        }

        public static IServiceCollection AddActorReference<TActor>(this IServiceCollection services, string name = null)
            where TActor : ActorBase
        {
            services.AddScoped<TActor>();

            services.AddSingleton<IActorRefProvider<TActor>>(sp =>
            {
                var actorSystem = sp.GetService<ActorSystem>();
                var actor = actorSystem.ActorOf(actorSystem.DI().Props<TActor>(), name ?? typeof(TActor).Name.ToLower());
                return new ActorRefProvider<TActor>(actor);
            });

            return services;
        }


        public static IServiceCollection AddActorReference<TInterface, TActor>(this IServiceCollection services, string name = null)
            where TActor : ActorBase, TInterface 
        {
            services.AddScoped<TActor>();

            services.AddSingleton<IActorRefProvider<TInterface>>(sp =>
            {
                var actorSystem = sp.GetService<ActorSystem>();
                var actor = actorSystem.ActorOf(actorSystem.DI().Props<TActor>(), name ?? typeof(TActor).Name.ToLower());
                return new ActorRefProvider<TInterface>(actor);
            });

            return services;
        }

        public static IServiceCollection AddCreator<TActor>(this IServiceCollection services)
            where TActor : ActorBase
        {
            services.AddScoped<TActor>();

            services.AddScoped<Creator<TActor>>(sp =>
            {
                return (context) => context.DI().Props<TActor>();
            });

            return services;
        }

        public static IServiceCollection AddCreator<TInterface, TActor>(this IServiceCollection services)
            where TActor : ActorBase, TInterface
        {
            services.AddScoped<TActor>();

            services.AddScoped<Creator<TInterface>>(sp =>
            {
                return (context) => context.DI().Props<TActor>();
            });

            return services;
        }
    }
}