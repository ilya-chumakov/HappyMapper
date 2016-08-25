using System;

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
}
