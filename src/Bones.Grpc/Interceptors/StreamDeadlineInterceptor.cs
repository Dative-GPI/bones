using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Bones.Grpc
{
    // https://stackoverflow.com/questions/63633332/grpc-what-are-the-best-practices-for-long-running-streaming
    public class StreamDeadlineInterceptor : Interceptor
    {
        private const int STREAM_DEADLINE_IN_SECONDS = 1800; // 60s * 30min

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = context;
            var deadline = DateTime.UtcNow.AddSeconds(STREAM_DEADLINE_IN_SECONDS);

            if (
                (!context.Options.Deadline.HasValue ||
                context.Options.Deadline.Value >= deadline)
            )
            {
                var newOptions = context.Options.WithDeadline(deadline);
                newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
            }

            var response = base.AsyncServerStreamingCall(request, newContext, continuation);
            return response;
        }
    }
}