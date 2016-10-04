using HappyMapper.Benchmarks.Types;
using NUnit.Framework;

namespace HappyMapper.Benchmarks
{
    public class Benchmark_Tests
    {
        [SetUp]
        public void SetUp()
        {
            Benchmark = new Benchmark<Src, Dest>();

            Benchmark.Register<HandwrittenMapper>();
            Benchmark.Register<HappyMapperSingle>();
            Benchmark.Register<EmitMapperSingle>();

            Benchmark.Register<HappyMapperCached>();
            Benchmark.Register<EmitMapperCached>();

        }

        public Benchmark<Src, Dest> Benchmark { get; set; }

        //[Test]
        public void Run_AllMappers_MeasuresTime()
        {
            int[] exponents = new[] { 7 };
            //int[] exponents = new[] { 5, 6, 7, 8 };

            Benchmark.Run(exponents);
        }
    }
}
