using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface ISuccessHandler<in TRequest> where TRequest: IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    public interface ISuccessHandler<in TRequest, TResult> where TRequest: IRequest<TResult>
    {
        Task HandleAsync(TRequest request, TResult result, CancellationToken cancellationToken);
    }
}