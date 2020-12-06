using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bones.Flow.Tests
{
    public class AssertHandler<TRequest> : IMiddleware<TRequest> where TRequest : IRequest
    {
        private Func<TRequest, bool> _assertion;

        public AssertHandler(Func<TRequest, bool> assertion)
        {
            _assertion = assertion;
        }

        public Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
        {
            if(!_assertion(request))
            {
                throw new ArgumentException("Assertion is false");
            }
            return next();
        }
    }
}