using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Akka.Actor;

namespace Bones.Akka
{
    public abstract class BaseManager<TMessage, TChildActor> : ReceiveActor
    {
        protected ILogger Logger { get; }
        protected IServiceProvider ServiceProvider { get; }

        public BaseManager(IServiceProvider sp, ILogger logger)
        {
            Logger = logger;
            ServiceProvider = sp;

            Receive<TMessage>(OnMessageReceived);

            Logger.LogInformation("Started at {path}", Self.Path.ToString());
        }

        private void OnMessageReceived(TMessage message)
        {
            var childName = GetChildName(message);

            if (string.IsNullOrWhiteSpace(childName))
            {
                Logger.LogWarning("Ignoring message because no child name was provided in message : {@message}", message);
                return;
            }

            var child = Context.Child(childName);

            if (child.IsNobody())
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var creator = scope.ServiceProvider.GetRequiredService<Creator<TChildActor>>();

                    Logger.LogInformation("Creating child with name {childName}", childName);
                    child = Context.ActorOf(creator(Context), childName);
                }
            }

            child.Forward(message);
        }

        protected abstract string GetChildName(TMessage message);

        protected override void PreRestart(Exception reason, object message)
        {
            Logger.LogError(reason, "Restarting actor at {path}", Self.Path.ToString());

            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            Logger.LogInformation("Stopped at {path}", Self.Path.ToString());
        }
    }
}
