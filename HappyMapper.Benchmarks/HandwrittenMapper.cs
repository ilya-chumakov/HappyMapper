using System;
using HappyMapper.Benchmarks.Types;

namespace HappyMapper.Benchmarks
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

        public Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new()
        {
            return (Action<Src, Dest>) Map as Action<TSrc, TDest>;
        }
    }
}