namespace Bones.Flow
{
    public interface IResponsibilityChain<TRequest> : ICommandHandler<TRequest> where TRequest : IRequest
    {
        void SetFirst(IResponsibilityChainLink<TRequest> firstWrapper);
        string ToString();
    }
}