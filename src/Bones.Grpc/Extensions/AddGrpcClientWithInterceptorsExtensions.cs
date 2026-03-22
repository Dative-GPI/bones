using System;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Grpc
{
    public static class AddGrpcClientWithInterceptorsExtensions
    {
        public static IHttpClientBuilder AddGrpcClientWithInterceptors<TClient>(
            this IServiceCollection services,
            Action<IServiceProvider, GrpcClientFactoryOptions> configureClient
        ) where TClient : class
        {
            return services.AddGrpcClient<TClient>(configureClient)
                .AddInterceptor<NotFoundInterceptor>();
                // .AddInterceptor<DeadlineInterceptor>()
                // .AddInterceptor<StreamDeadlineInterceptor>();
        }
    }
}