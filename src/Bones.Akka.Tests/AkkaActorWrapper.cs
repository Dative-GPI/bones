using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace Bones.Akka.Tests
{
    public class AkkaActorWrapper
    {
        public IActorRef ActorRef;

        public AkkaActorWrapper(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }

        public async Task<bool> Exists(TimeSpan timeout)
        {
            try
            {
                var identity = await ActorRef.Ask<ActorIdentity>(new Identify(null), timeout);
                return identity.Subject != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasChild(string name, TimeSpan timeout)
        {
            var selection = new ActorSelection(ActorRef, name);

            try
            {
                var actorRef = await selection.ResolveOne(timeout);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActorRef> GetChildOrDefault(string name, TimeSpan timeout)
        {
            var selection = new ActorSelection(ActorRef, name);

            try
            {
                var actorRef = await selection.ResolveOne(timeout);
                return actorRef;
            }
            catch
            {
                return default(IActorRef);
            }
        }

        public void Tell(object message, IActorRef sender)
        {
            ActorRef.Tell(message, sender);
        }
    }
}
