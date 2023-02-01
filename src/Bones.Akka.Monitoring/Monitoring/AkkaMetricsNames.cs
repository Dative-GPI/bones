namespace Bones.Akka.Monitoring
{
    public static class AkkaMetricsNames
    {
        public const string CREATED_ACTOR_COUNTER = "akka.actor.created";
        public const string RESTARTED_ACTOR_COUNTER = "akka.actor.restarted";
        public const string STOPPED_ACTOR_COUNTER = "akka.actor.stopped";
        public const string RECEIVED_MESSAGE_COUNTER = "akka.message.recv";
        public const string UNHANDLED_MESSAGE_COUNTER = "akka.message.unhandled";
        public const string MESSAGE_LATENCY_HISTOGRAM = "akka.message.latency";
        public const string MAILBOX_GAUGE = "akka.mailbox";
        public const string ACTOR_TYPE_LABEL = "actor_type";
        public const string ACTOR_PATH_LABEL = "actor_path";
        public const string MESSAGE_TYPE_LABEL = "actor_type";
        public const string SENDER_LABEL = "sender_path";
        public const string EXCEPTION_TYPE_LABEL = "exception_type";
        public const string MESSAGE_ACTIVITY_LABEL = "akka.msg.recv";
    }
}