using Microsoft.Extensions.DependencyInjection;
using Bones.Selectors.Interfaces;

namespace Bones.Selectors.DI
{
    public static class DependencyInjector
    {

        public static IServiceCollection AddSelectors(this IServiceCollection services)
        {
            services.AddScoped<IJsonSelector, JsonSelector>();
            services.AddScoped<IXmlSelector, XmlSelector>();

            return services;
        }
    }
}