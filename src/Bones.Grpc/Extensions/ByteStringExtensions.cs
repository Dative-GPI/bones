using Google.Protobuf;

namespace Bones.Grpc.Extensions {
    public static class ByteStringExtensions
    {
        public static ByteString ToByteString(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return ByteString.Empty;

            return ByteString.CopyFrom(byteArray);
        }
    }
}