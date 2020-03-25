using Akka.Actor;

namespace Bones.Akka
{
    public interface IActorRefProvider
    {
        IActorRef ActorRef { get; }
    }

    public interface IActorRefProvider<T> : IActorRefProvider
    {
    }
}