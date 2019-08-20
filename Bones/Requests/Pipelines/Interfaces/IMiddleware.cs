using System;
using System.Threading;
using System.Threading.Tasks;
using Chronos.Domain.Requests;

namespace Chronos.Domain.Pipelines.Interfaces
{
    public interface IMiddleware<in TRequest>
    {
        Task<RequestResult> HandleAsync(TRequest request, Func<Task<RequestResult>> next, CancellationToken cancellationToken);
    }
}