using HappyMapper.Benchmarks.Types;
using NUnit.Framework;

namespace HappyMapper.Benchmarks
{
    public class BenchmarkRunner
    {
        public void SetUp()
        {
            Benchmark = new Benchmark<Src, Dest>();

            Benchmark.Register<HandwrittenMapper>();
            Benchmark.Register<HappyMapperTyped>();
            Benchmark.Register<EmitMapperTyped>();

            Benchmark.Register<HappyMapperUntyped>();
            Benchmark.Register<EmitMapperUntyped>();
        }

        public Benchmark<Src, Dest> Benchmark { get; set; }

        public void RunAllMappersAndMeasureTime()
        {
            int[] exponents = new[] { 6 };
            //int[] exponents = new[] { 5, 6, 7, 8 };

            Benchmark.Run(exponents);
        }
    }
}
