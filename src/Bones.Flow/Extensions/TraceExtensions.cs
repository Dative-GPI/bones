namespace Bones.Flow
{
    public static class TraceExtensions
    {
        const string PIPELINE_REQUEST = "pipeline.request";
        const string PIPELINE_RESULT = "pipeline.result";
        const string PIPELINE_NODE_TYPE = "pipeline.nodeType";
        const string PIPELINE = "pipeline";
        const string MIDDLEWARE = "middleware";
        const string FAILUREHANDLER = "failurehandler";
        const string SUCCESSHANDLER = "successhandler";


        public static ITrace CreatePipelineTrace<TRequest>(this ITraceFactory factory) 
            where TRequest: IRequest
        {
            var trace = factory.Create(PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);
            }

            return trace;
        }

        public static ITrace CreatePipelineTrace<TRequest, TResult>(this ITraceFactory factory)
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest>(this ITraceFactory factory, IMiddleware<TRequest> middleware, ITrace pipelineTrace)
            where TRequest: IRequest
        {
            var trace = factory.Create(middleware.GetType().Name, pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest, TResult>(this ITraceFactory factory, IMiddleware<TRequest, TResult> middleware, ITrace pipelineTrace)
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(middleware.GetType().Name, pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);
            }

            return trace;
        }

        public static ITrace CreateFailureHandlerTrace<TRequest>(this ITraceFactory factory, IFailureHandler<TRequest> failureHandler, ITrace pipelineTrace) where TRequest: IRequest
        {
            var trace = factory.Create(failureHandler.GetType().Name, pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, FAILUREHANDLER);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest>(this ITraceFactory factory, ISuccessHandler<TRequest> successHandler, ITrace pipelineTrace) where TRequest: IRequest
        {
            var trace = factory.Create(successHandler.GetType().Name, pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest, TResult>(this ITraceFactory factory, ISuccessHandler<TRequest, TResult> successHandler, ITrace pipelineTrace) 
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(successHandler.GetType().Name, pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).Name);
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).Name);
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);
            }

            return trace;
        }
    }
}