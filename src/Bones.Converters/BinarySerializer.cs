using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Bones.Converters
{
    public static class BinarySerializer
    {
        public static byte[] StringsToBytes(Array content)
        {

            var serializedList = new List<byte[]>();

            long totalSize = 0;

            foreach(string s in content)
            {
                var size = Encoding.UTF8.GetByteCount(s);

                totalSize += (4 + size);

                serializedList.Add(BitConverter.GetBytes(size));
                serializedList.Add(Encoding.UTF8.GetBytes(s));
                
            }

            var serializedContent = new byte[totalSize];

            long nextIndex = 0;

            foreach(var s in serializedList)
            {
                Array.Copy(s, 0, serializedContent, nextIndex, Convert.ToInt64(s.Length));
                nextIndex+= s.LongLength;
            }

            return serializedContent;

        }

        public static byte[] ToBytes(Array content)
        {
            var type = content.GetValue(new int[content.Rank]).GetType();

            var getBytes = type == typeof(byte) ? null : typeof(BitConverter)
                .GetMethod(nameof(BitConverter.GetBytes), new Type[] { type });

            Func<object, byte[]> convert = (input) => new byte[] { };

            if(type == typeof(string))
            {
                return StringsToBytes(content);
            }

            if (type == typeof(byte))
            {
                convert = (input) => new byte[] { (byte)input };
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    convert = (input) =>
                    {
                        var array = (byte[])getBytes.Invoke(null, new object[] { input });
                        Array.Reverse(array);
                        return array;
                    };
                }
                else
                {
                    convert = (input) => (byte[])getBytes.Invoke(null, new object[] { input });
                }

            }

            int elemSize = Marshal.SizeOf(type);

            byte[] result = new byte[content.Length * elemSize];

            int nextIndex = 0;

            foreach (var input in content)
            {

                var bytecontent = convert(input);

                for (int j = 0; j < bytecontent.Length; j++)
                {
                    result[nextIndex + j] = bytecontent[j];
                }

                nextIndex += elemSize;
            }

            return result;

        }

        public static Array FromBuffer(byte[] rawContent, long[] shape, Type type)
        {
            var content = Array.CreateInstance(type, shape);

            int elemSize = 0;

            Func<byte[], object> convert = (input) => 0;


            if (type == typeof(byte))
            {
                elemSize = sizeof(byte);
                convert = (input) => input;

            }
            else if (type == typeof(float))
            {
                elemSize = sizeof(float);
                convert = (input) => BitConverter.ToSingle(input, 0);
            }
            else if (type == typeof(double))
            {
                elemSize = sizeof(double);
                convert = (input) => BitConverter.ToDouble(input, 0);
            }
            else if (type == typeof(int))
            {
                elemSize = sizeof(int);
                convert = (input) => BitConverter.ToInt32(input, 0);
            }
            else if (type == typeof(long))
            {
                elemSize = sizeof(long);
                convert = (input) => BitConverter.ToInt64(input, 0);
            }
            else if (type == typeof(char))
            {
                elemSize = sizeof(char);
                convert = (input) => BitConverter.ToInt16(input, 0);
            }
            else if (type == typeof(bool))
            {
                elemSize = sizeof(bool);
                convert = (input) => BitConverter.ToBoolean(input, 0);
            }
            else if (type == typeof(string))
            {

            }
            else
            {
                throw new NotImplementedException(String.Format("Type {0} is not supported", type.ToString()));
            }


            long[] calculateNextPos(long[] next)
            {
                for (int j = next.Length - 1; j > 0; j--)
                {
                    if (next[j] == shape[j])
                    {
                        next[j] = 0;
                        next[j - 1] += 1;
                    }
                }

                return next;
            }

            long[] nextPos = new long[content.Rank];

            if (type == typeof(string))
            {
                using (var reader = new BinaryReader(new MemoryStream(rawContent)))
                {
                    

                    while (reader.PeekChar() != -1)
                    {
                        nextPos = calculateNextPos(nextPos);

                        var stringSize = Convert.ToInt32(reader.ReadUInt32());

                        var s = new string(reader.ReadChars(stringSize));

                        content.SetValue(s, nextPos);

                        nextPos[nextPos.Length - 1] += 1;
                    }
                }

            }
            else
            {
                byte[] buffer = new byte[elemSize];

                for (int i = 0; i < rawContent.Length; i += elemSize)
                {
                    nextPos = calculateNextPos(nextPos);

                    Array.Copy(rawContent, i, buffer, 0, elemSize);

                    if (BitConverter.IsLittleEndian) Array.Reverse(buffer);

                    content.SetValue(convert(buffer), nextPos);

                    nextPos[nextPos.Length - 1] += 1;

                }
            }

            return content;

        }
        public static byte[] ToBytesLegacy(Array content)
        {
            var type = content.GetValue(new int[content.Rank]).GetType();

            Func<object, byte[]> convert = (input) => new byte[] { };

            int elemSize = Marshal.SizeOf(type);

            if (type == typeof(byte))
            {
                elemSize = sizeof(byte);
                convert = (input) => new byte[] { (byte)input };

            }
            else if (type == typeof(float))
            {
                elemSize = sizeof(float);
                convert = (input) => BitConverter.GetBytes((float)input);
            }
            else if (type == typeof(double))
            {
                elemSize = sizeof(double);
                convert = (input) => BitConverter.GetBytes((double)input);
            }
            else if (type == typeof(int))
            {
                elemSize = sizeof(int);
                convert = (input) => BitConverter.GetBytes((int)input);
            }
            else if (type == typeof(long))
            {
                elemSize = sizeof(long);
                convert = (input) => BitConverter.GetBytes((long)input);
            }
            else if (type == typeof(char))
            {
                elemSize = sizeof(char);
                convert = (input) => BitConverter.GetBytes((char)input);
            }
            else if (type == typeof(bool))
            {
                elemSize = sizeof(bool);
                convert = (input) => BitConverter.GetBytes((bool)input);
            }
            else
            {
                throw new NotImplementedException(String.Format("Type {0} is not supported", type.ToString()));
            }

            byte[] result = new byte[content.Length * elemSize];

            int nextIndex = 0;

            Func<object, byte[]> getBytes = (input) => new byte[] { };


            if (BitConverter.IsLittleEndian)
            {

                getBytes = (input) =>
                {

                    var b = convert(input);
                    Array.Reverse(b);
                    return b;

                };
            }
            else
            {
                getBytes = convert;
            }


            foreach (var input in content)
            {

                var byteContent = getBytes(input);

                for (int j = 0; j < byteContent.Length; j++)
                {
                    result[nextIndex + j] = byteContent[j];
                }

                nextIndex += elemSize;
            }


            return result;

        }
    }
}
