using Akka.Actor;

namespace Bones.Akka
{
    public class ActorRefProvider<T> : ActorRefProvider, IActorRefProvider<T>
    {
        public ActorRefProvider(IActorRef actorRef)
            : base(actorRef)
        {
        }
    }

    public abstract class ActorRefProvider : IActorRefProvider
    {
        public IActorRef ActorRef { get; }
        protected ActorRefProvider(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }
    }
}