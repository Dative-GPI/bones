namespace Bones.Akka.Messages
{
    public class RestartMessage
    {
        private static readonly RestartMessage _instance = new RestartMessage();
        public static RestartMessage Instance
        {
            get { return _instance; }
        }

        static RestartMessage() { }

        private RestartMessage() { }
    }
}
