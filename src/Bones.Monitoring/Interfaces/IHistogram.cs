using System.Collections.Generic;

namespace Bones.Monitoring
{
    public interface IHistogram<T> where T : struct
    {
        void Add(T value, KeyValuePair<string, object>[] tags = null);
    }
}