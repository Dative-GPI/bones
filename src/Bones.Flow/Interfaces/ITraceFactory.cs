namespace Bones.Flow
{
    public interface ITraceFactory
    {
        ITrace Create(string name, ITrace parent = null);
    }
}