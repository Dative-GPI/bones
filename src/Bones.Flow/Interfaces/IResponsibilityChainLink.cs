using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IResponsibilityChainLink<TRequest>
    {
        void SetNext(IResponsibilityChainLink<TRequest> nextWrapper);
        void SetHandler(IResponsibilityChainHandler<TRequest> handler);
        Task HandleAsync(TRequest request, CancellationToken token);
    }
}