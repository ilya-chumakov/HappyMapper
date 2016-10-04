using System;
using OrdinaryMapper.Benchmarks.Types;

namespace OrdinaryMapper.Benchmarks
{
    public class HandwrittenMapper : ITestableMapper
    {
        public static HandwrittenMapper Instance => new HandwrittenMapper();

        public static void Map(Src src, Dest dest)
        {
            dest.Name = src.Name;
            dest.DateTime = src.DateTime;
            dest.Float = src.Float;
            dest.Number = src.Number;
        }

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            return (Action<Src, Dest>) Map as Action<TInput, TOutput>;
        }
    }
}