using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace Bones.Akka
{
    public static class IActorRefProviderExtensions
    {
        public static void Tell(this IActorRefProvider provider, object message, IActorRef sender)
        {
            provider.ActorRefs.Tell(message, sender);
        }

        public static void Tell(this IActorRefProvider provider, object message)
        {
            provider.ActorRefs.Tell(message, ActorRefs.NoSender);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, TimeSpan? timeout = null)
        {
            return provider.ActorRefs.Ask(message, timeout);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, CancellationToken cancellationToken)
        {
            return provider.ActorRefs.Ask(message, cancellationToken);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRefs.Ask(message, timeout, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, TimeSpan? timeout = null)
        {
            return provider.ActorRefs.Ask<T>(message, timeout);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, CancellationToken cancellationToken)
        {
            return provider.ActorRefs.Ask<T>(message, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRefs.Ask<T>(message, timeout, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, Func<IActorRef, object> messageFactory, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRefs.Ask<T>(messageFactory, timeout, cancellationToken);
        }
    }
}