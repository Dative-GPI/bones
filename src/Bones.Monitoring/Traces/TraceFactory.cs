using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bones.Monitoring.Core
{
    public class TraceFactory : ITraceFactory
    {
        private ILogger<TraceFactory> _logger;
        private IOptionsMonitor<BonesMonitoringOptions> _options;

        public TraceFactory(ILogger<TraceFactory> logger, IOptionsMonitor<BonesMonitoringOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        public ITrace Create(ActivitySource source, string name, ITrace parent = null)
        {
            Activity activity;

            if (parent != null && !(parent is Trace))
            {
                throw new System.ApplicationException("You shouldn't use specific trace without implementing our own ITraceFactory");
            }

            if (parent != null && parent.IsRecording && parent is Trace trace)
            {
                var context = new ActivityContext(
                    ActivityTraceId.CreateFromString(parent.TraceId),
                    ActivitySpanId.CreateFromString(parent.SpanId),
                    parent.IsRecording ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None
                );

                activity = source.StartActivity(
                    name,
                    ActivityKind.Internal,
                    context
                );
            }
            else
            {
                activity = source.StartActivity(
                    name,
                    ActivityKind.Internal
                );
            }

            var result = new Trace(activity);
            return result;
        }

        public ITrace Enrich(ITrace trace, object param, string optionsName)
        {
            var option = _options.Get(optionsName);
            if(option != null && option.SpanEnricher != null) option.SpanEnricher(trace, param);
            return trace;
        }
    }
}