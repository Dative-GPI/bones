using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Bones.Monitoring.Core.Metrics
{
    public class MetricFactory : IMetricFactory
    {
        private ConcurrentDictionary<string, object> counters;

        public MetricFactory()
        {
            counters = new ConcurrentDictionary<string, object>();
        }

        public ICounter<T> GetCounter<T>(string key, Meter meter, string unit = null, string description = null) where T : struct
        {
            var counter = counters.GetOrAdd(key, new CounterMetric<T>(meter, key, unit, description));
            if(counter is CounterMetric<T> typedCounter)
            {
                return typedCounter;
            }
            else
            {
                throw new System.InvalidCastException($"Counter with key {key} is not of type {typeof(T)}");
            }
        }
    }
}