using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IQueryHandler<in TRequest, TResponse> where TRequest: IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}