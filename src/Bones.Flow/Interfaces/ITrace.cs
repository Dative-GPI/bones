using System;

namespace Bones.Flow
{
    public interface ITrace : IDisposable
    {
        bool IsRecording { get; }
        string TraceId { get; }
        string SpanId { get; }

        void Stop();
        void Start();
        void SetTag(string key, object value);
    }
}