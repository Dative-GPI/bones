using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IResponsibilityChainHandler<TRequest>
    {
        Task<bool> HandleAsync(TRequest request);
    }
}