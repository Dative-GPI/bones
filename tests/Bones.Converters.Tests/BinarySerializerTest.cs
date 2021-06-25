using System;
using Xunit;
using Bones.Converters;

namespace Bones.Converters.Tests
{
    public class BinarySerializerTests
    {
        [Fact]
        public void TestToBytes()
        {

            var expectedResult = new byte[] { 0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,1,0,0,0,2};

            var test = new int[2,3]{{1,2,3},{4,1,2}};

            var result = BinarySerializer.ToBytes(test);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestStringToBytes()
        {

            var expectedResult = new byte[] { 1,0,0,0, 97, 2,0,0,0, 98,99, 1,0,0,0, 100, 1,0,0,0, 101};

            var test = new string[2,2]{{"a", "bc"},{"d", "e"}};

            var result = BinarySerializer.ToBytes(test);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestFromBuffer()
        {

            var test = new byte[] { 0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,1,0,0,0,2};

            var expectedResult = new int[2,3]{{1,2,3},{4,1,2}};

            var result = BinarySerializer.FromBuffer(test, new long[]{2,3}, typeof(int));

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestStringFromBuffer()
        {

            var test = new byte[] { 1,0,0,0, 97, 2,0,0,0, 98,99, 1,0,0,0, 100, 1,0,0,0, 101};

            var expectedResult = new string[2,2]{{"a", "bc"},{"d", "e"}};

            var result = BinarySerializer.FromBuffer(test, new long[]{2,2}, typeof(string));

            Assert.Equal(expectedResult, result);
        }
    }
}
