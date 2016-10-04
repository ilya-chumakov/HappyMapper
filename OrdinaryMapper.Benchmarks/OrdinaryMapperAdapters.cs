using System;

namespace OrdinaryMapper.Benchmarks
{
    /// <summary>
    /// Mapper search + mapping.
    /// New API and text engine.
    /// Perfomance a bit degraded due to 'if' statements added by new text engine (runtime null checks etc.).
    /// </summary>
    public class OrdinaryMapperCachedV3 : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<TInput, TOutput>();
            });

            var mapper = config.CompileMapper();

            Action<TInput, TOutput> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// Mapping without cache search
    /// </summary>
    public class OrdinaryMapperSingleV2 : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<TInput, TOutput>();
            });

            var mapper = config.CompileMapper();

            var singleMapper = mapper.GetSingleMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

}
