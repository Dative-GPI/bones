using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Bones.Monitoring
{
    public interface IMetricFactory
    {
        ICounter<T> GetCounter<T>(string key, Meter meter, string unit = null, string description = null) where T : struct;
    }
}