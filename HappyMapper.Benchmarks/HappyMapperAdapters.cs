using System;

namespace HappyMapper.Benchmarks
{
    /// <summary>
    /// Search in cache + mapping.
    /// </summary>
    public class HappyMapperUntyped : ITestableMapper
    {
        public Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<TSrc, TDest>();
            });

            var mapper = config.CompileMapper();

            Action<TSrc, TDest> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// Direct mapping without search in cache.
    /// </summary>
    public class HappyMapperTyped : ITestableMapper
    {
        public Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<TSrc, TDest>();
            });

            var mapper = config.CompileMapper();

            var singleMapper = mapper.GetSingleMapper<TSrc, TDest>();

            Action<TSrc, TDest> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

}
