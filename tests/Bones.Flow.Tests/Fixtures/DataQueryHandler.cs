using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class DataQueryHandler : IMiddleware<DataQuery, IEnumerable<string>>
    {
        public Task<IEnumerable<string>> HandleAsync(DataQuery request, Func<Task<IEnumerable<string>>> next, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "test", "succeed" });
        }
    }
}