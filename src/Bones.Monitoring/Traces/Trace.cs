using System;
using System.Diagnostics;

namespace Bones.Monitoring.Core
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