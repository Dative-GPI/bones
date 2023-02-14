using Bones.Flow;
using Demo.Akka.Monitoring.Console.Abstractions;

namespace Demo.Akka.Monitoring.Console.Middlewares
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