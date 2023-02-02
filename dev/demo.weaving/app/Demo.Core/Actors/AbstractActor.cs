
using Akka.Actor;
using Demo.Core.Messages;
using Demo.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo.Core.Actors
{
    public abstract class AbstractActor : ReceiveActor, IAbstractActor
    {
        public AbstractActor(IServiceProvider sp)
        {
        }

        protected abstract void DemoMehodWithoutBody();

        protected override void PreStart()
        {
            base.PreStart();
        }

        protected override void PostStop()
        {
            base.PostStop();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
        }
    }
}