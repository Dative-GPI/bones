using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Akka.Actor;
using Bones.Monitoring;

namespace Bones.Akka.Monitoring {
    public class AkkaMonitor
    {
        private Meter _akkaMeter;
        private ITraceFactory _traceFactory;
        public ConcurrentDictionary<string, ActorCounters> counters = new ConcurrentDictionary<string, ActorCounters>();

        public AkkaMonitor(ITraceFactory traceFactory)
        {
            _traceFactory = traceFactory;
            _akkaMeter = new Meter(AkkaMetricsNames.AKKA_METER_NAME, "1.0.0");
        }

        public ActorCounters GetCounters(IActorContext context)
        {
            return counters.GetOrAdd(context.Self.Path.Address.ToString(), new ActorCounters(context, _akkaMeter));
        }
    }
}