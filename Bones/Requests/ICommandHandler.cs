using System.Threading;
using System.Threading.Tasks;

namespace Bones.Requests
{
    public interface ICommandHandler<in TRequest>
    {
        Task<RequestResult> HandleAsync(TRequest request, CancellationToken cancellationToken, bool commit = true);
    }
}