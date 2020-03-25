using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Requests.Pipelines.Interfaces
{
    public interface IMiddleware<in TRequest>
    {
        Task<RequestResult> HandleAsync(TRequest request, Func<Task<RequestResult>> next, CancellationToken cancellationToken);
    }
}