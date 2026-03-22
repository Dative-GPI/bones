using Microsoft.Extensions.DependencyInjection;

namespace Bones.Grpc.DI
{
    public static class DependencyInjector
    {
        public static IServiceCollection AddBonesGrpc(this IServiceCollection services)
        {
            services.AddTransient<NotFoundInterceptor>();
            services.AddTransient<DeadlineInterceptor>();
            services.AddTransient<StreamDeadlineInterceptor>();

            return services;
        }
    }
}