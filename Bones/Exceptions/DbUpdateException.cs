namespace Bones.Exceptions
{
    [System.Serializable]
    public class DbUpdateException : CustomException
    {
        public DbUpdateException() { }
        public DbUpdateException(string message) : base(message) { }
        public DbUpdateException(string message, System.Exception inner) : base(message, inner) { }
        protected DbUpdateException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}