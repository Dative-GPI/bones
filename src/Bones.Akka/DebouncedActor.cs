using System;

using Akka.Actor;

namespace Bones.Akka
{
    public abstract class DebouncedActor : ReceiveActor, IWithTimers
    {
        const string DEBOUNCER = "DEBOUNCER";

        public ITimerScheduler Timers { get; set; }

        protected void Debounce<TMessage>(TMessage message, TimeSpan? delay = null)
        {
            Timers.StartSingleTimer(
                DEBOUNCER,
                new DebouncedMessage<TMessage> { Content = message },
                delay ?? TimeSpan.FromMilliseconds(500)
            );
        }

        protected void Debounce<TMessage>(string key, TMessage message, TimeSpan? delay = null)
        {
            Timers.StartSingleTimer(
                key,
                new DebouncedMessage<TMessage> { Content = message },
                delay ?? TimeSpan.FromMilliseconds(500)
            );
        }

        protected class DebouncedMessage<T>
        {
            public T Content { get; set; }
        }
    }
}
