namespace Bones.Monitoring
{
    public interface IRequestSerializer
    {
        string Serialize(object request);
    }
}