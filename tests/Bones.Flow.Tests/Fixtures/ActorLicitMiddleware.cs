using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bones.Flow.Tests
{
    public class ActorLicitMiddleware : IMiddleware<IActorRequest>
    {
        public async Task HandleAsync(IActorRequest request, Func<Task> next, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(request.ActorId))
            {
                throw new ArgumentNullException(nameof(request.ActorId));
            }

            await next();
        }
    }
}