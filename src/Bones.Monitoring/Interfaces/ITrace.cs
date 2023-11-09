using System;

namespace Bones.Monitoring
{
    public interface ITrace : IDisposable
    {
        bool IsRecording { get; }
        string TraceId { get; }
        string SpanId { get; }

        void Stop();
        void Start();
        void SetTag(string key, object value);
        void SetError(Exception exception, string data = null);
    }
}