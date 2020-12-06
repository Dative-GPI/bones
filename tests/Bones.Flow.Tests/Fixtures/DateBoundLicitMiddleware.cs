using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class DateBoundLicitMiddleware<TResult> : IMiddleware<IDateBoundedRequest<TResult>, IEnumerable<TResult>>
    {
        public Task<IEnumerable<TResult>> HandleAsync(IDateBoundedRequest<TResult> request, Func<Task<IEnumerable<TResult>>> next, CancellationToken cancellationToken)
        {
            if(request.DateMax < request.DateMin)
            {
                return Task.FromResult(Enumerable.Empty<TResult>());
            }
            return next();
        }
    }
}