using System;

namespace Bones.Monitoring.Core
{
    public class BonesMonitoringOptions
    {
        public Action<ITrace, object> SpanEnricher { get; set; }
    }
}