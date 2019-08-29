using System;

namespace Bones.Exceptions
{
    [System.Serializable]
    public class NullResultException : CustomException
    {
        public NullResultException() { }
        public NullResultException(string message) : base(message) { }
        public NullResultException(string message, System.Exception inner) : base(message, inner) { }
        protected NullResultException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
