namespace Bones.Flow
{
    public interface IResponsibilityChainFactory<TRequest> where TRequest : IRequest
    {
        IResponsibilityChainFactory<TRequest> Add<THandler>() where THandler : IResponsibilityChainHandler<TRequest>;
        IResponsibilityChainFactory<TRequest> Add<THandler>(THandler handler) where THandler : IResponsibilityChainHandler<TRequest>;
        IResponsibilityChain<TRequest> Build();
    }
}