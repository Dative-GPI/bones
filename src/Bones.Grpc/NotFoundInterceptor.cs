using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Bones.Grpc
{
    public class NotFoundInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var response = base.AsyncUnaryCall(request, context, continuation);

            var responseWithErrorhandling = response.ResponseAsync.ContinueWith(
                r => {
                    try
                    {
                        var response = r.Result;
                        return response;
                    }
                    catch (AggregateException ex)
                    {
                        if(ex.InnerException is RpcException rpcException 
                            && rpcException.StatusCode == StatusCode.NotFound){
                            return null;
                        }
                        throw;
                    }
                }
            );
            
            return new AsyncUnaryCall<TResponse>(responseWithErrorhandling, response.ResponseHeadersAsync, response.GetStatus, response.GetTrailers, response.Dispose);
        }
    }
}
