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
            provider.ActorRef.Tell(message, sender);
        }

        public static void Tell(this IActorRefProvider provider, object message)
        {
            provider.ActorRef.Tell(message, ActorRefs.NoSender);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, TimeSpan? timeout = null)
        {
            return provider.ActorRef.Ask(message, timeout);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, CancellationToken cancellationToken)
        {
            return provider.ActorRef.Ask(message, cancellationToken);
        }

        public static Task<object> Ask(this IActorRefProvider provider, object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRef.Ask(message, timeout, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, TimeSpan? timeout = null)
        {
            return provider.ActorRef.Ask<T>(message, timeout);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, CancellationToken cancellationToken)
        {
            return provider.ActorRef.Ask<T>(message, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRef.Ask<T>(message, timeout, cancellationToken);
        }

        public static Task<T> Ask<T>(this IActorRefProvider provider, Func<IActorRef, object> messageFactory, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return provider.ActorRef.Ask<T>(messageFactory, timeout, cancellationToken);
        }
    }
}