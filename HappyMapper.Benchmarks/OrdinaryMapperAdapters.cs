using System;

namespace HappyMapper.Benchmarks
{
    /// <summary>
    /// Mapper search + mapping.
    /// New API and text engine.
    /// Perfomance a bit degraded due to 'if' statements added by new text engine (runtime null checks etc.).
    /// </summary>
    public class HappyMapperCached : ITestableMapper
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
    /// Mapping without cache search
    /// </summary>
    public class HappyMapperSingle : ITestableMapper
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
