using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HappyMapper.Benchmarks
{
    public class Benchmark<TSrc, TDest> 
        where TSrc : class, new() where TDest : class, new()
    {
        public int NameMaxLength { get; private set; }
        public Dictionary<string, Action<TSrc, TDest>> Mappers { get; set; }

        public Benchmark()
        {
            Mappers = new Dictionary<string, Action<TSrc, TDest>>();
        }

        public void Register<TMapper>() where TMapper : ITestableMapper, new ()
        {
            ITestableMapper mapper = new TMapper();

            Mappers.Add(mapper.GetType().Name, mapper.CreateMapMethod<TSrc, TDest>());
        }

        public void Run(int[] exponents)
        {
            NameMaxLength = Mappers.Keys.Max(k => k.Length);

            Console.Write("Exponents:  ");
            Array.ForEach(exponents, e => Console.Write(e + " "));
            Console.WriteLine();
            Console.WriteLine("--------------------------");

            foreach (var kvp in Mappers)
            {
                Console.Write(MapperNameFormatted(kvp.Key));

                Action<TSrc, TDest> mapMethod = kvp.Value;

                GC.Collect();

                //warmup
                mapMethod(new TSrc(), new TDest());
                
                foreach (int exponent in exponents)
                {
                    int iterationCount = (int)Math.Pow(10, exponent);

                    var stopwatch = new Stopwatch();
                    for (int i = 0; i < iterationCount; i++)
                    {
                        var src = new TSrc();
                        var dest = new TDest();

                        stopwatch.Start();
                        mapMethod(src, dest);
                        stopwatch.Stop();
                    }
                    Console.Write(stopwatch.ElapsedMilliseconds + " ");
                }
                Console.WriteLine();

                GC.Collect();
                Thread.Sleep(TimeSpan.FromSeconds(5));
                GC.Collect();
            }
            Console.WriteLine("--------------------------");
        }

        private string MapperNameFormatted(string key)
        {
            return key + new String(' ', NameMaxLength - key.Length + 2);
        }
    }
}
