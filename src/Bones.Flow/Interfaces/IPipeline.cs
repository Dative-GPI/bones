using System.Collections.Generic;
using Bones.Flow.Core;

namespace Bones.Flow
{
    /// <summary>
    /// La pipeline peut-être utilisé comme commandhandler (sans résultat), ou comme un middleware
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IPipeline<TRequest> : ICommandHandler<TRequest>, IMiddleware<TRequest>
        where TRequest : IRequest
    {
        void Configure(IEnumerable<IMiddleware<TRequest>> middlewares,
            IEnumerable<ISuccessHandler<TRequest>> successHandlers,
            IEnumerable<IFailureHandler<TRequest>> failureHandlers,
            IUnitOfWork unitOfWork = null);
    }

    public interface IPipeline<TRequest, TResult> :
        ICommandHandler<TRequest, TResult>,
        IQueryHandler<TRequest, TResult>,
        IMiddleware<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        void Configure(List<MiddlewareType> middlewareTypes, 
            List<IMiddleware<TRequest>> requestMiddlewares, 
            List<IMiddleware<TRequest, TResult>> requestResultMiddlewares, 
            List<ISuccessHandler<TRequest>> requestSuccessHandler, 
            List<ISuccessHandler<TRequest, TResult>> requestResultSuccessHandler, 
            List<IFailureHandler<TRequest>> failureHandlers,
            IUnitOfWork uow = null);
    }
}