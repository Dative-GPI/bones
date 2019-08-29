using System;

namespace Bones.Exceptions
{
    [System.Serializable]
    public class CustomException : System.Exception
    {
        public CustomException() { }
        public CustomException(string message) : base(message) { }
        public CustomException(string message, System.Exception inner) : base(message, inner) { }
        protected CustomException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
