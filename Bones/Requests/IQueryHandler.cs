using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Requests
{
    public interface IQueryHandler<in TRequest>
    {
        Task<RequestResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}