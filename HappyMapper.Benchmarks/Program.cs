using System;

namespace HappyMapper.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var benchmark = new Benchmark_Tests();
                benchmark.SetUp();
                benchmark.Run_AllMappers_MeasuresTime();

                Console.Read(); 
            }
        }
    }
}