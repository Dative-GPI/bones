using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace Bones.Monitoring.Core.Tracing
{
    public class Trace : ITrace
    {
        private Activity _activity;

        public Trace(Activity activity)
        {
            _activity = activity;
        }

        public bool IsRecording => _activity != null && _activity.IsAllDataRequested;

        public string TraceId => _activity?.TraceId.ToString();

        public string SpanId => _activity?.SpanId.ToString();


        public void Dispose()
        {
            _activity?.Stop();
            _activity?.Dispose();
        }

        public void SetError(Exception ex, string data = null)
        {
            _activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            var tags = new ActivityTagsCollection(new Dictionary<string, object>
            {
                { "Type", ex.GetType().FullName },
                { "Message", ex.Message },
                { "StackTrace", ex.StackTrace },
            });
            if(!string.IsNullOrWhiteSpace(data))
            {
                tags.Add("Data", data);
            }
            var activityEvent = new ActivityEvent("Exception", DateTime.UtcNow, tags);
            _activity?.AddEvent(activityEvent);
        }

        public void SetTag(string key, object value)
        {
            _activity?.SetTag(key, value);
        }

        public void Start()
        {
            _activity?.Start();
        }

        public void Stop()
        {
            _activity?.Stop();
        }
    }
}