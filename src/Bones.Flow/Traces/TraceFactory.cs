using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Bones.Flow.Core
{
    internal class TraceFactory : ITraceFactory
    {
        static ActivitySource activitySource = new ActivitySource(
            "Bones.Flow.Core",
            "semver1.0.0");
        private ILogger<TraceFactory> _logger;

        public TraceFactory(ILogger<TraceFactory> logger)
        {
            _logger = logger;
        }

        public ITrace Create(string name, ITrace parent = null)
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

                activity = activitySource.StartActivity(
                    name,
                    ActivityKind.Internal,
                    context
                );
            }
            else
            {
                activity = activitySource.StartActivity(
                    name,
                    ActivityKind.Internal
                );
            }

            return new Trace(activity);
        }
    }
}