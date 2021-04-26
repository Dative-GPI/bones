using Akka.Actor;

namespace Bones.Akka
{
    public class ActorRefProvider<T> : ActorRefProvider, IActorRefProvider<T>
    {
        public ActorRefProvider(ActorSelection actorRefs)
            : base(actorRefs)
        {
        }
    }

    public abstract class ActorRefProvider : IActorRefProvider
    {
        public ActorSelection ActorRefs { get; }
        protected ActorRefProvider(ActorSelection actorRefs)
        {
            ActorRefs = actorRefs;
        }
    }
}