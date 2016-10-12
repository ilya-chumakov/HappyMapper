using System;
using EmitMapper;

namespace HappyMapper.Benchmarks
{
    /// <summary>
    /// Mapper search + mapping
    /// </summary>
    public class EmitMapperUntyped : ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperUntyped();

        public Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new()
        {
            Action<TSrc, TDest> action = (src, dest) =>
            {
                var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TSrc, TDest>();
                mapper.Map(src, dest);
            };

            return action;
        }
    }

    /// <summary>
    /// Mapping without cache search
    /// </summary>
    public class EmitMapperTyped : ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperTyped();

        public Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new()
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TSrc, TDest>();

            Action<TSrc, TDest> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }
}