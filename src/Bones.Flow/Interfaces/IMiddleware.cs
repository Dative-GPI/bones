using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IMiddleware<in TRequest> where TRequest: IRequest
    {
        Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken);
    }

    public interface IMiddleware<in TRequest, TResult> where TRequest: IRequest<TResult>
    {
        Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken);
    }

}