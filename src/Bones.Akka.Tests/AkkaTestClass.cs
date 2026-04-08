using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit;
using Bones.Tests;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Bones.Akka.Tests
{
    public class AkkaTestClass : TestKit
    {
        protected static readonly TimeSpan STANDARD_TIMEOUT = TimeSpan.FromMilliseconds(150);

        // https://gist.github.com/Havret/78409e91c9adf62aed3392574a3d0446
        protected TestScheduler Scheduler => (TestScheduler)Sys.Scheduler;
        protected ITestOutputHelper _output;

        public AkkaTestClass(ITestOutputHelper output)
            : base(@"akka.scheduler.implementation = ""Akka.TestKit.TestScheduler, Akka.TestKit""")
        {
            _output = output;
        }

        public AkkaActorWrapper CreateWrappedActor<T>(
            Expression<Func<T>> constructor,
            string name,
            TestProbe parent = null
        )
            where T : ActorBase
        {
            IActorRef actorRef;

            if (parent == null)
                actorRef = Sys.ActorOf(Props.Create(constructor), name);
            else
                actorRef = parent.ChildActorOf(Props.Create(constructor), name);

            return new AkkaActorWrapper(actorRef);
        }

        public AkkaActorWrapper CreateWrappedActor<T>(
            Expression<Func<T>> constructor,
            string name,
            IUntypedActorContext context
        )
            where T : ActorBase
        {
            IActorRef actorRef;

            actorRef = context.ActorOf(Props.Create(constructor), name);

            return new AkkaActorWrapper(actorRef);
        }

        public ILogger<T> CreateLogger<T>()
        {
            return new XunitLogger<T>(_output);
        }

        public Creator<T> CreateProxyCreator<T>(TestProbe probe)
        {
            return ctx =>
                Props.Create(() => new ProxyNodeActor(CreateLogger<ProxyNodeActor>(), probe));
        }

    }
}
