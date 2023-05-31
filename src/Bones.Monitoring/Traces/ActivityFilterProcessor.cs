using System;
using System.Diagnostics;
using OpenTelemetry;

namespace Bones.Monitoring.Core.Tracing
{
   public class ActivityFilterProcessor : BaseProcessor<Activity>
    {
        private readonly Func<Activity, bool> filter;
        public ActivityFilterProcessor(Func<Activity, bool> filter)
        {
            this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public override void OnEnd(Activity activity)
        {
            // Bypass export if the Filter returns false.
            if (!this.filter(activity))
            {
                activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
            }
        }
    }
}