using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Bones.Monitoring.Core.Metrics
{
    public class CounterMetric<T>: ICounter<T> where T : struct
    {
        private Counter<T> counter;

        public CounterMetric(Meter meter, string key, string unit = null, string description = null)
        {
            counter = meter.CreateCounter<T>(key, unit, description);
        }

        public void Add(T value, KeyValuePair<string, object>[] tags = null)
        {
            counter.Add(value, tags);
        }
    }
}