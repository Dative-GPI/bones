using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.Actor;
using Bones.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Akka.Monitoring
{
    public class MonitoredReceiveActor : ReceiveActor
    {
        private ITraceFactory _traceFactory;
        private ActorCounters _counters;

        public MonitoredReceiveActor(IServiceProvider sp)
        {
            _traceFactory = sp.GetRequiredService<ITraceFactory>();

            var akkaMonitor = sp.GetRequiredService<AkkaMonitor>();
            _counters = akkaMonitor.GetCounters(Context);
            _counters.IncrementCreatedActorsCounter();
        }

        protected void MonitoredReceiveAsync<T>(Func<T, Task> handler, Predicate<T> shouldHandle = null)
        {
            ReceiveAsync(MonitoredFunc<T>(handler), shouldHandle);
        }

        private Func<T, Task> MonitoredFunc<T>(Func<T, Task> handler)
        {
            var messageTag = new KeyValuePair<string, object>(AkkaMetricsNames.MESSAGE_TYPE_LABEL, typeof(T).ToColloquialString());
            return async (param) =>
            {
                using (var activity = _traceFactory.CreateActorMessageTrace<T>(Context, param))
                {
                    _counters.IncrementMessagesCounter(messageTag);
                    _counters.UpdateMessageQueueCounter((Context as ActorCell).NumberOfMessages);
                    var timer = Stopwatch.StartNew();
                    await handler(param);
                    timer.Stop();
                    _counters.RecordMessageLatency(timer.ElapsedMilliseconds, messageTag);
                }
            };
        }

        protected void MonitoredReceive<T>(Action<T> handler, Predicate<T> shouldHandle = null)
        {
            Receive<T>(MonitoredAction<T>(handler), shouldHandle);
        }


        private Action<T> MonitoredAction<T>(Action<T> handler)
        {
            var messageTag = new KeyValuePair<string, object>(AkkaMetricsNames.MESSAGE_TYPE_LABEL, typeof(T).ToColloquialString());
            return (param) =>
            {
                using (var activity = _traceFactory.CreateActorMessageTrace<T>(Context, param))
                {
                    _counters.IncrementMessagesCounter(messageTag);
                    _counters.UpdateMessageQueueCounter((Context as ActorCell).NumberOfMessages);
                    var timer = Stopwatch.StartNew();
                    handler(param);
                    timer.Stop();
                    _counters.RecordMessageLatency(timer.ElapsedMilliseconds, messageTag);
                }
            };
        }

        protected override void Unhandled(object message)
        {
            _counters.IncrementUnhandledMessagesCounter(new KeyValuePair<string, object>(AkkaMetricsNames.MESSAGE_TYPE_LABEL, message.GetType().ToColloquialString()));
            base.Unhandled(message);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _counters.IncrementRestartedActorsCounter(new KeyValuePair<string, object>(AkkaMetricsNames.EXCEPTION_TYPE_LABEL, reason.GetType().ToColloquialString()));
            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            _counters.UpdateMessageQueueCounter(0);
            _counters.IncrementStoppedActorsCounter();
            base.PostStop();
        }
    }
}