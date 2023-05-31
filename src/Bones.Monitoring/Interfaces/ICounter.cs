using System.Collections.Generic;

namespace Bones.Monitoring
{
    public interface ICounter<T> where T : struct
    {
        void Add(T value, KeyValuePair<string, object>[] tags = null);
    }
}