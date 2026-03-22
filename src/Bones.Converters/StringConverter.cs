using System;
using System.Text;

namespace Bones.Converters
{
    public static class StringConverter
    {
        public static byte[] ToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even number of characters.", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string FromBytes(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
