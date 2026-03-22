namespace Bones.Akka.Messages
{
    public class DisableMessage
    {
        private static readonly DisableMessage _instance = new DisableMessage();
        public static DisableMessage Instance
        {
            get { return _instance; }
        }

        static DisableMessage() { }

        private DisableMessage() { }
    }
}
