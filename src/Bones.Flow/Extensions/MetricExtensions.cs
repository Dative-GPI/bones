using System.Collections.Generic;
using Bones.Monitoring;

namespace Bones.Flow
{
    public static class MetricExtensions
    {
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