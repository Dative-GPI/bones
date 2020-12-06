namespace Bones.Flow
{
    public interface IRequest 
    { 

    }
    public interface IRequest<in TResult> : IRequest
    {
    }
}