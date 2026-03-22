namespace Bones.Selectors
{
    [System.Serializable]
    public class SelectorException : System.Exception
    {
        public SelectorException() { }
        public SelectorException(string message) : base(message) { }
        public SelectorException(string message, System.Exception inner) : base(message, inner) { }
        protected SelectorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}