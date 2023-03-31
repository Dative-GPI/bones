using System.Diagnostics;
using System.Collections.Generic;
using Akka.Actor;
using Bones.Monitoring;

using static Bones.Akka.Monitoring.Consts;

namespace Bones.Akka.Monitoring
{
    public static class TraceExtensions
    {
        static ActivitySource activitySource = new ActivitySource(
            BONES_AKKA_MONITORING_INSTRUMENTATION,
            "1.0.0");
        static List<IActorRef> actors = new List<IActorRef>();

        const string ACTOR_MESSAGE = "actor.message";
        const string ACTOR_TYPE = "actor.type";
        const string SENDER_TYPE = "sender.type";
        const string ACTOR_PATH = "actor.path";

        public static ITrace CreateActorMessageTrace<TMessage>(this ITraceFactory factory, IActorContext context, object param)
        {
            var trace = factory.Create(activitySource, typeof(TMessage).ToColloquialString());

            if (trace.IsRecording)
            {
                trace.SetTag(ACTOR_MESSAGE, typeof(TMessage).ToColloquialString());
                trace.SetTag(SENDER_TYPE, context.Sender.Path.ToString());//Not sure of that, if the context is modified before access to the value 
                trace.SetTag(ACTOR_TYPE, context.Props.Type);
                trace.SetTag(ACTOR_PATH, context.Self.Path.ToString());
                
                trace = factory.Enrich(trace, param, BONES_AKKA_MONITORING_INSTRUMENTATION);
            }

            return trace;
        }

        // private static Action<ITrace, object> Enrich {get; set;}

        // public static void EnrichWith(Action<ITrace, object> enricher)
        // {
        //     Enrich = enricher;
        // }
    }
}