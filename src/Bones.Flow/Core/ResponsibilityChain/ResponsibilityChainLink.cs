using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bones.Flow
{
    internal class ResponsibilityChainLink<TRequest> : IResponsibilityChainLink<TRequest>
    {
        private ILogger<ResponsibilityChainLink<TRequest>> _logger;
        private IResponsibilityChainLink<TRequest> _nextWrapper;
        private IResponsibilityChainHandler<TRequest> _handler;

        public ResponsibilityChainLink(
            ILogger<ResponsibilityChainLink<TRequest>> logger
        )
        {
            _logger = logger;
        }

        public void SetNext(
            IResponsibilityChainLink<TRequest> nextWrapper
        )
        {
            _nextWrapper = nextWrapper;
        }

        public void SetHandler(
            IResponsibilityChainHandler<TRequest> handler
        )
        {
            _handler = handler;
        }

        public async Task HandleAsync(TRequest request, CancellationToken token)
        {
            var handled = false;

            try
            {
                if (_handler != null) handled = await _handler.HandleAsync(request);
            }
            catch
            {
                throw;
            }

            if (handled || _nextWrapper == null) return;
            else await _nextWrapper.HandleAsync(request, token);
        }

        public override string ToString()
        {
            return (_handler == null ? "null" : _handler.ToString()) + " -> " + (_nextWrapper == null ? "null" : _nextWrapper.ToString());
        }
    }
}