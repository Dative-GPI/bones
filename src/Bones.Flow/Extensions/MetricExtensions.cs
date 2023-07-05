using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Bones.Flow.Core;
using Bones.Monitoring;

namespace Bones.Flow
{
    public static class MetricExtensions
    {
        public static readonly Meter METER = new Meter(Consts.BONES_FLOW_METER, "1.0.0");

        public static void Record<TRequest>(this IHistogram<double> histogram, long elapsedMilliseconds)
            where TRequest : IRequest
        {
            histogram.Add(elapsedMilliseconds, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("request", typeof(TRequest).ToColloquialString()) });
        }

        public static void Record(this IHistogram<double> histogram, long elapsedMilliseconds, string type)
        {
            histogram.Add(elapsedMilliseconds, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("type", type) });
        }
    }
}