
using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Bones.Grpc
{
    public class DeadlineInterceptor : Interceptor
    {
        private const double DEADLINE_IN_SECONDS = 5;

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = context;
            var deadline = DateTime.UtcNow.AddSeconds(DEADLINE_IN_SECONDS);

            if (
                context.Options.CancellationToken == default &&
                (!context.Options.Deadline.HasValue ||
                context.Options.Deadline.Value >= deadline)
            )
            {
                var newOptions = context.Options.WithDeadline(deadline);
                newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
            }

            var call = continuation(request, newContext);

            return new AsyncUnaryCall<TResponse>(
                call.ResponseAsync,
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose
            );
        }
    }
}