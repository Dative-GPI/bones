using System.Diagnostics;

namespace Bones.Monitoring
{
    public interface ITraceFactory
    {
        ITrace Create(ActivitySource source, string name, ITrace parent = null);
    }
}