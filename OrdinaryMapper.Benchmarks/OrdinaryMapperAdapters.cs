using System;
using OrdinaryMapper.Obsolete;

namespace OrdinaryMapper.Benchmarks
{
    /// <summary>
    /// Mapping without cache search
    /// </summary>
    public class OrdinaryMapperSingle : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var genericMapper = new Mapper();
            genericMapper.CreateMap<TInput, TOutput>();
            genericMapper.Compile();

            var singleMapper = genericMapper.GetSingleMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// Mapper search + mapping
    /// </summary>
    public class OrdinaryMapperCached : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var genericMapper = new Mapper();
            genericMapper.CreateMap<TInput, TOutput>();
            genericMapper.Compile();

            Action<TInput, TOutput> action = (src, dest) => genericMapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// New API and text engine.
    /// Perfomance a bit degraded due to 'if' statements added by new text engine (runtime null checks etc.).
    /// </summary>
    public class OrdinaryMapperCachedV2 : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TInput, TOutput>();
            });

            var mapper = config.CompileMapper();

            Action<TInput, TOutput> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }
}
