using System;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Flow.Console.Commands;

namespace Demo.Flow.Console.Handlers
{
    public class BoolQueryHandler : IMiddleware<BoolQuery, bool>
    {
        public async Task<bool> HandleAsync(BoolQuery request, Func<Task<bool>> next, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("Handle" + request.Message);
            
            return await Task.Run(() => request.Message == "Hello World");
        }
    }
}