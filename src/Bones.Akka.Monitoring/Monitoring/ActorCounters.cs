using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Akka.Actor;

namespace Bones.Akka.Monitoring
{
    public class ActorCounters
    {
        private Meter _akkaMeter;
        private KeyValuePair<string, object> actorPathTag;
        private KeyValuePair<string, object> actorTypeTag;
        private Counter<int> receivedMessagesCounter;
        private Counter<int> createdActorCounter;
        private Counter<int> restartedActorCounter;
        private Counter<int> stoppedActorCounter;
        private Counter<int> unhandledMessagesCounter;
        private ObservableGauge<int> messageQueueCounter;
        private Histogram<long> messagesLatencyHistogram;
        private Counter<int> logsCounter; //Not implemented yet
        private Counter<int> deadLettersCounter; //Not implemented yet
        private int currentNumberOfMessages { get; set; }
        
        public ActorCounters(IActorContext context, Meter meter)
        {
            actorTypeTag = new KeyValuePair<string, object>(AkkaMetricsNames.ACTOR_TYPE_LABEL, context.Props.TypeName);
            actorPathTag = new KeyValuePair<string, object>(AkkaMetricsNames.ACTOR_PATH_LABEL, context.Self.Path.ToString());

            receivedMessagesCounter = meter.CreateCounter<int>(AkkaMetricsNames.RECEIVED_MESSAGE_COUNTER, description: "count the number of messages received by each message type / actor type pair.");
            createdActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.CREATED_ACTOR_COUNTER, description: "counts the number of actor restarts over time.");
            restartedActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.RESTARTED_ACTOR_COUNTER, description: "counts the number of actors started.");
            stoppedActorCounter = meter.CreateCounter<int>(AkkaMetricsNames.STOPPED_ACTOR_COUNTER, description: "counts the number of actors stopped.");
            messageQueueCounter = meter.CreateObservableGauge<int>(AkkaMetricsNames.MAILBOX_GAUGE, 
                () => new Measurement<int>(this.currentNumberOfMessages, actorTypeTag, actorPathTag), description: "a gauge that measures the queue length of the actor. Should be enabled");
            unhandledMessagesCounter = meter.CreateCounter<int>(AkkaMetricsNames.UNHANDLED_MESSAGE_COUNTER, description: "counts the number of Unhandled messages by message type.");
            messagesLatencyHistogram = meter.CreateHistogram<long>(AkkaMetricsNames.MESSAGE_LATENCY_HISTOGRAM, description: "a timer that messages the latency for each message / actor type pair; uses the same end to end measurement that tracing does.");
            // logsCounter = _akkaMeter.CreateCounter<int>("akka.logs", description: "counts the number of logs of each log level into a collection. Also collections the Exception types raised in the error logs");
            // deadLettersCounter = _akkaMeter.CreateCounter<int>("akka.messages.deadletters", description: "counts the number of DeadLetter messages by message type.");
        }

        public void UpdateMessageQueueCounter(int numberOfMessages)
        {
            this.currentNumberOfMessages = numberOfMessages;
        }

        public void IncrementCreatedActorsCounter()
        {
            receivedMessagesCounter.Add(1, actorTypeTag, actorPathTag);
        }

        public void IncrementMessagesCounter(KeyValuePair<string, object> messageTag)
        {
            receivedMessagesCounter.Add(1, actorTypeTag, actorPathTag, messageTag);
        }

        public void RecordMessageLatency(long latency, KeyValuePair<string, object> messageType)
        {
            messagesLatencyHistogram.Record(latency, actorTypeTag, actorPathTag, messageType);
        }

        public void IncrementRestartedActorsCounter(KeyValuePair<string, object> exceptionTypeTag)
        {
            restartedActorCounter.Add(1, actorTypeTag, actorPathTag, exceptionTypeTag);
        }

        public void IncrementStoppedActorsCounter()
        {
            stoppedActorCounter.Add(1, actorTypeTag, actorPathTag);
        }

        public void IncrementUnhandledMessagesCounter(KeyValuePair<string, object> messageTag)
        {
            unhandledMessagesCounter.Add(1, actorTypeTag, actorPathTag, messageTag);
        }
    }


}