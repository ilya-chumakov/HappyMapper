using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace OrdinaryMapper.Benchmarks
{
    public class Benchmark_Tests
    {

        [SetUp]
        public void SetUp()
        {
            Benchmark = new Benchmark<Src, Dest>();

            Benchmark.Register<SingleMapperAdapter>();
            Benchmark.Register<TakeSingleMapperFromCacheAdapter>();
            Benchmark.Register<GenericMapAdapter>();

            Benchmark.Register<HandwrittenMapper>();

            Benchmark.Register<EmitMapperSingle>();
            Benchmark.Register<EmitMapperCached>();
        }

        public Benchmark<Src, Dest> Benchmark { get; set; }

        [Test]
        public void Run_AllMappers_MeasuresTime()
        {
            int[] exponents = new[] { 6, 7 };
            //int[] exponents = new[] { 5, 6, 7, 8 };

            Benchmark.Run(exponents);
        }
    }
}
