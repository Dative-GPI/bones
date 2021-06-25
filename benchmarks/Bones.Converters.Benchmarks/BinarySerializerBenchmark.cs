using System;
using System.Linq;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

using Bones.Converters;


namespace Bones.Converters.Benchmarks
{
    public class BinarySerializerBenchmark
    {
    
        [Params(2,3)]
        public int DimNumber;

        [Params(10,100)]
        public int DimLength;


        private Array data;

        [GlobalSetup(Targets = new[]{nameof(Serialization), nameof(SerializationLegacy)})]
        public void SerializationSetup()
        {

            var shape = new int[DimNumber];

            shape = shape.Select(e => DimLength).ToArray();

            data = Array.CreateInstance(typeof(double), shape);
        }

        private byte[] rawData;
        private long[] shape;

        [Benchmark]
        public byte[] Serialization()
        {
            return BinarySerializer.ToBytes(data);
        }

        [Benchmark]
        public byte[] SerializationLegacy()
        {
            return BinarySerializer.ToBytesLegacy(data);
        }

        [GlobalSetup(Target = nameof(DeSerialization))]
        public void DeSerializationSetup()
        {
            shape = new long[DimNumber];

            shape = shape.Select(e => (long)DimLength).ToArray();

            rawData = new byte[DimLength * DimNumber * sizeof(double)];
        }

        [Benchmark]
        public Array DeSerialization()
        {
            return BinarySerializer.FromBuffer(rawData, shape, typeof(double));
        }


    }
}