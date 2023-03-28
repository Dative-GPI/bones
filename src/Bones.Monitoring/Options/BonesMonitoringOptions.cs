using System;

namespace Bones.Monitoring.Core
{
    public class BonesMonitoringOptions
    {
        public string Name {get; set;}
        public Action<ITrace, object> SpanEnricher { get; set; }
    }
}