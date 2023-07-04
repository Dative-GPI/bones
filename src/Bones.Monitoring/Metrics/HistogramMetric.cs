using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Bones.Monitoring.Core.Metrics
{
    public class HistogramMetric<T>: IHistogram<T> where T : struct
    {
        private Histogram<T> counter;

        public HistogramMetric(Meter meter, string key, string unit = null, string description = null)
        {
            counter = meter.CreateHistogram<T>(key, unit, description);
        }

        public void Add(T value, KeyValuePair<string, object>[] tags = null)
        {
            counter.Record(value, tags);
        }
    }
}