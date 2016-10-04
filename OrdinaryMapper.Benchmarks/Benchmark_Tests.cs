using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;

namespace OrdinaryMapper.Benchmarks
{
    public class Benchmark_Tests
    {

        [SetUp]
        public void SetUp()
        {
            Benchmark = new Benchmark<Src, Dest>();

            Benchmark.Register<OrdinaryMapperSingleV2>();
            Benchmark.Register<OrdinaryMapperCachedV3>();

            Benchmark.Register<EmitMapperCached>();
            Benchmark.Register<EmitMapperSingle>();

            Benchmark.Register<HandwrittenMapper>();
        }

        public Benchmark<Src, Dest> Benchmark { get; set; }

        [Test]
        public void Run_AllMappers_MeasuresTime()
        {
            int[] exponents = new[] { 6 };
            //int[] exponents = new[] { 5, 6, 7, 8 };

            Benchmark.Run(exponents);
        }
    }
}
