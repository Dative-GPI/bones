using System;
using System.Diagnostics;
using Bones;
using Bones.Monitoring;

namespace Bones.Flow
{
    public static class TraceExtensions
    {
        static ActivitySource activitySource = new ActivitySource(
            "Bones.Flow.Core",
            "semver1.0.0");

        const string PIPELINE_REQUEST = "pipeline.request";
        const string PIPELINE_RESULT = "pipeline.result";
        const string PIPELINE_NODE_TYPE = "pipeline.nodeType";
        const string PIPELINE = "pipeline";
        const string MIDDLEWARE = "middleware";
        const string FAILUREHANDLER = "failurehandler";
        const string SUCCESSHANDLER = "successhandler";
        const string COMMIT = "commit";


        public static ITrace CreatePipelineTrace<TRequest>(this ITraceFactory factory) 
            where TRequest: IRequest
        {
            var trace = factory.Create(activitySource, PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);
            }

            return trace;
        }

        public static ITrace CreatePipelineTrace<TRequest, TResult>(this ITraceFactory factory)
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(activitySource, PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest>(this ITraceFactory factory, IMiddleware<TRequest> middleware, ITrace pipelineTrace)
            where TRequest: IRequest
        {
            var trace = factory.Create(activitySource, middleware.GetType().ToColloquialString(), pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest, TResult>(this ITraceFactory factory, IMiddleware<TRequest, TResult> middleware, ITrace pipelineTrace)
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(activitySource, middleware.GetType().ToColloquialString(), pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);
            }

            return trace;
        }

        public static ITrace CreateCommitTrace(this ITraceFactory factory, ITrace pipelineTrace)
        {
            var trace = factory.Create(activitySource, COMMIT, pipelineTrace);

            return trace;
        }

        public static ITrace CreateFailureHandlerTrace<TRequest>(this ITraceFactory factory, IFailureHandler<TRequest> failureHandler, ITrace pipelineTrace) where TRequest: IRequest
        {
            var trace = factory.Create(activitySource, failureHandler.GetType().ToColloquialString(), pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, FAILUREHANDLER);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest>(this ITraceFactory factory, ISuccessHandler<TRequest> successHandler, ITrace pipelineTrace) where TRequest: IRequest
        {
            var trace = factory.Create(activitySource, successHandler.GetType().ToColloquialString(), pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest, TResult>(this ITraceFactory factory, ISuccessHandler<TRequest, TResult> successHandler, ITrace pipelineTrace) 
            where TRequest: IRequest<TResult>
        {
            var trace = factory.Create(activitySource, successHandler.GetType().ToColloquialString(), pipelineTrace);

            if(trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);
            }

            return trace;
        }
    }
}