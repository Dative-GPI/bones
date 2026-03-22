using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bones.Flow.Core
{
    internal class ResponsibilityChain<TRequest> :
        IResponsibilityChain<TRequest>
        where TRequest : IRequest
    {
        IResponsibilityChainLink<TRequest> _firstLink;
        ILogger<ResponsibilityChain<TRequest>> _logger;

        public ResponsibilityChain(
            ILogger<ResponsibilityChain<TRequest>> logger
        )
        {
            _logger = logger;
        }

        public void SetFirst(
            IResponsibilityChainLink<TRequest> firstLink
        )
        {
            Contract.Assert(firstLink != null);
            _firstLink = firstLink;
        }

        public async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default, bool commit = true)
        {
            try
            {
                if (_firstLink != null)
                    await _firstLink.HandleAsync(request, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occured executing ResponsibilityChain<{TRequest}>", typeof(TRequest).Name);
                throw;
            }
        }

        public override string ToString()
        {
            return "Chain = " + _firstLink.ToString();
        }
    }
}