using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface ICommandHandler<in TRequest> where TRequest : IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken = default(CancellationToken), bool commit = true);
    }
    
    public interface ICommandHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default(CancellationToken), bool commit = true);
    }
}