using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace Bones.Akka.Tests
{
    public class ProxyNodeActor : ReceiveActor
    {
        private ILogger<ProxyNodeActor> _logger;
        private IActorRef _probe;

        public ProxyNodeActor(ILogger<ProxyNodeActor> logger, IActorRef probe)
        {
            _logger = logger;
            _probe = probe;

            ReceiveAny(m => _probe.Tell(m));
        }
    }
}