using System;
using System.Threading;
using System.Threading.Tasks;
using Chronos.Domain.Requests;

namespace Chronos.Domain.Requests
{
    public interface IQueryHandler<in TRequest>
    {
        Task<RequestResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}