using System.Collections.Generic;

namespace Bones.Flow
{
    public interface IChainOfResponsibility<TRequest> : ICommandHandler<TRequest>, IMiddleware<TRequest>
        where TRequest : IRequest
    {
        void Configure(List<IChainOfResponsibilityHandler<TRequest>> handlers);
    }

    public interface IChainOfResponsibility<TRequest, TResult> : ICommandHandler<TRequest, TResult>, IMiddleware<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        void Configure(List<IChainOfResponsibilityHandler<TRequest, TResult>> handlers);
    }
}
