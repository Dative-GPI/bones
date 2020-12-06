using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class NotDefaultMiddleware<TResult> : IMiddleware<IRequest<TResult>, TResult>
    {
        public async Task<TResult> HandleAsync(IRequest<TResult> request, Func<Task<TResult>> next, CancellationToken cancellationToken)
        {
            var result = await next();

            if(EqualityComparer<TResult>.Default.Equals(result, default(TResult)))
            {
                throw new Exception();
            }

            return result;
        }
    }
}