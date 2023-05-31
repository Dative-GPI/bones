using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bones.Flow;
using Demo.Flow.Console.Abstractions;

namespace Demo.Flow.Console.Middlewares
{
    public class WriteLineLicitMiddleware : IMiddleware<IWriteLineRequest>
    {
        public async Task HandleAsync(IWriteLineRequest request, Func<Task> next, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("Message licit middleware");

            if (String.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentNullException(nameof(request.Message));
            }

            System.Console.WriteLine("MIDDLEWARE BEFORE " + Activity.Current.SpanId);
            await next();

            System.Console.WriteLine("MIDDLEWARE AFTER " + Activity.Current.OperationName);

            var activity = Program.LOCAL_SOURCE.StartActivity("midactivty");


            System.Console.WriteLine("MIDDLEWARE CREATED " + Activity.Current.SpanId);
            activity.Stop();
        }
    }
}