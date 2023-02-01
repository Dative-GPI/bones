using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Bones.Monitoring.Core
{
    public class TraceFactory : ITraceFactory
    {        private ILogger<TraceFactory> _logger;

        public TraceFactory(ILogger<TraceFactory> logger)
        {
            _logger = logger;
        }

        public ITrace Create(ActivitySource source, string name, ITrace parent = null)
        {
            Activity activity;

            if(parent != null && !(parent is Trace))
            {
                throw new System.ApplicationException("You shouldn't use specific trace without implementing our own ITraceFactory");
            }

            if (parent != null && parent.IsRecording && parent is Trace trace)
            {
                var context = new ActivityContext(
                    ActivityTraceId.CreateFromString(parent.TraceId),
                    ActivitySpanId.CreateFromString(parent.SpanId),
                    ActivityTraceFlags.None
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

            return new Trace(activity);
        }
    }
}