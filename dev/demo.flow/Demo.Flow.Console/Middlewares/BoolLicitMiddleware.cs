using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Flow.Console.Abstractions;
using Demo.Flow.Console.Commands;

namespace Demo.Flow.Console.Middlewares
{
    public class BoolLicitMiddleware : IMiddleware<BoolQuery, bool>
    {
        public async Task<bool> HandleAsync(BoolQuery request, Func<Task<bool>> next, CancellationToken cancellationToken)
        {


            System.Console.WriteLine("Bool licit middleware");
            System.Console.WriteLine("MIDDLEWARE BEFORE " + Activity.Current.SpanId);
            var result = await next();
            System.Console.WriteLine("MIDDLEWARE AFTER " + Activity.Current.SpanId);
            
            
            if (String.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentNullException(nameof(request.Message));
            }

            return result;
        }
    }
}