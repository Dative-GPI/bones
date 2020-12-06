using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class NotEmptyMiddleware<TResult> : IMiddleware<IRequest<IEnumerable<TResult>>, IEnumerable<TResult>>
    {

        public async Task<IEnumerable<TResult>> HandleAsync(IRequest<IEnumerable<TResult>> request, Func<Task<IEnumerable<TResult>>> next, CancellationToken cancellationToken)
        {
            var result = await next();

            if(!result.Any())
            {
                throw new Exception();
            }
            
            return result;
        }
    }
}