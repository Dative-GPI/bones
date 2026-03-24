using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Threading.Tasks;

namespace Bones.Grpc
{
    public class NotFoundInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(
                HandleResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> task)
        {
            try
            {
                return await task;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return default;
            }
        }
    }
}
