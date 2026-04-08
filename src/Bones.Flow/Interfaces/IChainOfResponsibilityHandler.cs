using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IChainOfResponsibilityHandler<in TRequest> where TRequest : IRequest
    {
        Task<bool> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    public interface IChainOfResponsibilityHandler<in TRequest, TResult> where TRequest : IRequest<TResult>
    {
        Task<(bool handled, TResult result)> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
