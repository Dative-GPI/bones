using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Akka.Actor;

namespace Bones.Akka.Monitoring
{
    public class ActorCounters
    {
        private KeyValuePair<string, object> _actorPathTag;
        private KeyValuePair<string, object> _actorTypeTag;
        private Counter<int> _receivedMessagesCounter;
        private Counter<int> _createdActorCounter;
        private Counter<int> _restartedActorCounter;
        private Counter<int> _stoppedActorCounter;
        private Counter<int> _unhandledMessagesCounter;
        private ObservableGauge<int> _messageQueueCounter;
        private Histogram<long> _messagesLatencyHistogram;
        // private Counter<int> _logsCounter; //Not implemented yet
        // private Counter<int> _deadLettersCounter; //Not implemented yet
        private int _currentNumberOfMessages;
        
        public ActorCounters(IActorContext context, Meter meter)
        {
            _actorTypeTag = new KeyValuePair<string, object>(AkkaMetricsNames.ACTOR_TYPE_LABEL, context.Props.Type.ToString());
            _actorPathTag = new KeyValuePair<string, object>(AkkaMetricsNames.ACTOR_PATH_LABEL, context.Self.Path.ToString());

            _receivedMessagesCounter = meter.CreateCounter<int>(AkkaMetricsNames.RECEIVED_MESSAGE_COUNTER, description: "count the number of messages received by each message type / actor type pair.");
            _createdActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.CREATED_ACTOR_COUNTER, description: "counts the number of actor restarts over time.");
            _restartedActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.RESTARTED_ACTOR_COUNTER, description: "counts the number of actors started.");
            _stoppedActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.STOPPED_ACTOR_COUNTER, description: "counts the number of actors stopped.");
            _messageQueueCounter = meter.CreateObservableGauge<int>(AkkaMetricsNames.MAILBOX_GAUGE, 
                () => new Measurement<int>(this._currentNumberOfMessages, _actorTypeTag, _actorPathTag), description: "a gauge that measures the queue length of the actor. Should be enabled");
            _unhandledMessagesCounter = meter.CreateCounter<int>(AkkaMetricsNames.UNHANDLED_MESSAGE_COUNTER, description: "counts the number of Unhandled messages by message type.");
            _messagesLatencyHistogram = meter.CreateHistogram<long>(AkkaMetricsNames.MESSAGE_LATENCY_HISTOGRAM, description: "a timer that messages the latency for each message / actor type pair; uses the same end to end measurement that tracing does.");
            // logsCounter = _akkaMeter.CreateCounter<int>("akka.logs", description: "counts the number of logs of each log level into a collection. Also collections the Exception types raised in the error logs");
            // deadLettersCounter = _akkaMeter.CreateCounter<int>("akka.messages.deadletters", description: "counts the number of DeadLetter messages by message type.");
        }

        public void UpdateMessageQueueCounter(int numberOfMessages)
        {
            this._currentNumberOfMessages = numberOfMessages;
        }

        public void IncrementCreatedActorsCounter()
        {
            _createdActorCounter.Add(1, _actorTypeTag, _actorPathTag);
        }

        public void IncrementMessagesCounter(KeyValuePair<string, object> messageTag)
        {
            _receivedMessagesCounter.Add(1, _actorTypeTag, _actorPathTag, messageTag);
        }

        public void RecordMessageLatency(long latency, KeyValuePair<string, object> messageType)
        {
            _messagesLatencyHistogram.Record(latency, _actorTypeTag, _actorPathTag, messageType);
        }

        public void IncrementRestartedActorsCounter(KeyValuePair<string, object> exceptionTypeTag)
        {
            _restartedActorCounter.Add(1, _actorTypeTag, _actorPathTag, exceptionTypeTag);
        }

        public void IncrementStoppedActorsCounter()
        {
            _stoppedActorCounter.Add(1, _actorTypeTag, _actorPathTag);
        }

        public void IncrementUnhandledMessagesCounter(KeyValuePair<string, object> messageTag)
        {
            _unhandledMessagesCounter.Add(1, _actorTypeTag, _actorPathTag, messageTag);
        }
    }
}