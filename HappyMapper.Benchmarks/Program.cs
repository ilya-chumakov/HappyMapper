using System;

namespace HappyMapper.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var runner = new BenchmarkRunner();
                runner.SetUp();
                runner.RunAllMappersAndMeasureTime();

                Console.Read(); 
            }
        }
    }
}