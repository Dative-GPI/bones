using System;
using System.Diagnostics;
using Bones;
using Bones.Monitoring;

using static Bones.Flow.Core.Consts;

namespace Bones.Flow
{
    public static class TraceExtensions
    {
        static ActivitySource activitySource = new ActivitySource(
            BONES_FLOW_INSTRUMENTATION,
            "semver1.0.0");

        const string PIPELINE_REQUEST = "pipeline.request";
        const string PIPELINE_RESULT = "pipeline.result";
        const string PIPELINE_NODE_TYPE = "pipeline.nodeType";
        const string PIPELINE = "pipeline";
        const string MIDDLEWARE = "middleware";
        const string FAILUREHANDLER = "failurehandler";
        const string SUCCESSHANDLER = "successhandler";
        const string COMMIT = "commit";
        const string AFTER = "_after";
        const string BEFORE = "_before";

        public static ITrace CreatePipelineTrace<TRequest>(this ITraceFactory factory, object param = null)
            where TRequest : IRequest
        {
            var trace = factory.Create(activitySource, PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreatePipelineTrace<TRequest, TResult>(this ITraceFactory factory, object param = null)
            where TRequest : IRequest<TResult>
        {
            var trace = factory.Create(activitySource, PIPELINE);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, PIPELINE);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest>(this ITraceFactory factory, IMiddleware<TRequest> middleware, ITrace pipelineTrace, Boolean after = false, object param = null)
            where TRequest : IRequest
        {
            var name = middleware.GetType().ToColloquialString();
            name += after ? AFTER : BEFORE;
            var trace = factory.Create(activitySource, name, pipelineTrace);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreateMiddlewareTrace<TRequest, TResult>(this ITraceFactory factory, IMiddleware<TRequest, TResult> middleware, ITrace pipelineTrace, Boolean after = false, object param = null)
            where TRequest : IRequest<TResult>
        {
            var name = middleware.GetType().ToColloquialString();
            name += after ? AFTER : BEFORE;
            var trace = factory.Create(activitySource, name, pipelineTrace);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, MIDDLEWARE);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreateCommitTrace(this ITraceFactory factory, ITrace pipelineTrace, object param = null)
        {
            var trace = factory.Create(activitySource, COMMIT, pipelineTrace);

            trace.Enrich(factory, param);

            return trace;
        }

        public static ITrace CreateFailureHandlerTrace<TRequest>(this ITraceFactory factory, IFailureHandler<TRequest> failureHandler, ITrace pipelineTrace, object param = null) where TRequest : IRequest
        {
            var trace = factory.Create(activitySource, failureHandler.GetType().ToColloquialString(), pipelineTrace);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, FAILUREHANDLER);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest>(this ITraceFactory factory, ISuccessHandler<TRequest> successHandler, ITrace pipelineTrace, object param = null) where TRequest : IRequest
        {
            var trace = factory.Create(activitySource, successHandler.GetType().ToColloquialString(), pipelineTrace);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        public static ITrace CreateSuccessHandlerTrace<TRequest, TResult>(this ITraceFactory factory, ISuccessHandler<TRequest, TResult> successHandler, ITrace pipelineTrace, object param = null)
            where TRequest : IRequest<TResult>
        {
            var trace = factory.Create(activitySource, successHandler.GetType().ToColloquialString(), pipelineTrace);

            if (trace.IsRecording)
            {
                trace.SetTag(PIPELINE_REQUEST, typeof(TRequest).ToColloquialString());
                trace.SetTag(PIPELINE_RESULT, typeof(TResult).ToColloquialString());
                trace.SetTag(PIPELINE_NODE_TYPE, SUCCESSHANDLER);

                trace.Enrich(factory, param);
            }

            return trace;
        }

        private static ITrace Enrich(this ITrace trace, ITraceFactory factory, object param)
        {
            if (trace.IsRecording)
            {
                trace = factory.Enrich(trace, param, BONES_FLOW_INSTRUMENTATION);
            }
            return trace;
        }
    }
}