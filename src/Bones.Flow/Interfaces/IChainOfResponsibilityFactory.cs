namespace Bones.Flow
{
    public interface IChainOfResponsibilityFactory<TRequest> where TRequest : IRequest
    {
        IChainOfResponsibilityFactory<TRequest> Add<THandler>()
            where THandler : IChainOfResponsibilityHandler<TRequest>;
        IChainOfResponsibilityFactory<TRequest> Add<THandler>(THandler handler)
            where THandler : IChainOfResponsibilityHandler<TRequest>;
        IChainOfResponsibility<TRequest> Build();
    }

    public interface IChainOfResponsibilityFactory<TRequest, TResult> where TRequest : IRequest<TResult>
    {
        IChainOfResponsibilityFactory<TRequest, TResult> Add<THandler>()
            where THandler : IChainOfResponsibilityHandler<TRequest, TResult>;
        IChainOfResponsibilityFactory<TRequest, TResult> Add<THandler>(THandler handler)
            where THandler : IChainOfResponsibilityHandler<TRequest, TResult>;
        IChainOfResponsibility<TRequest, TResult> Build();
    }
}
