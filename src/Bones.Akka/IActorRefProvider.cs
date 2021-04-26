using Akka.Actor;

namespace Bones.Akka
{
    public interface IActorRefProvider
    {
        ActorSelection ActorRefs { get; }
    }

    public interface IActorRefProvider<T> : IActorRefProvider
    {
    }
}