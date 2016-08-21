using System;

namespace OrdinaryMapper.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var benchmark = new BenchmarkV1();
            benchmark.SetUp();
            benchmark.Run_AllMappers_MeasuresTime();

            Console.Read();
        }
    }
}