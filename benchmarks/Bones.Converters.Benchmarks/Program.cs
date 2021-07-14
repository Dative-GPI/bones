using System;
using System.Linq;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace Bones.Converters.Benchmarks
{
    public class AllowNonOptimized : ManualConfig
    {
        public AllowNonOptimized()
        {
            AddValidator(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

            AddLogger(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
            //AddExporter(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
            AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BinarySerializerBenchmark>(new AllowNonOptimized());
        }
    }
}
