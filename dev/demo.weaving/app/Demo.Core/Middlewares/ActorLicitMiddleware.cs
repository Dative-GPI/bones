using Bones.Flow;
using Demo.Domain.Abstractions;

namespace Demo.Core.Middlewares
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