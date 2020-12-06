using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IFailureHandler<TRequest> where TRequest: IRequest
    {
        Task HandleAsync(TRequest request, Exception exception, CancellationToken cancellationToken);
    }
}