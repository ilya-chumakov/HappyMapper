using System;

namespace OrdinaryMapper.Benchmarks
{
    /// <summary>
    /// Mapping without cache search
    /// </summary>
    public class SingleMapperAdapter : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var genericMapper = new Mapper();

            var singleMapper = genericMapper.CreateMap<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// Mapping without cache search
    /// </summary>
    public class TakeSingleMapperFromCacheAdapter : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var genericMapper = new Mapper();

            genericMapper.CreateMap<TInput, TOutput>();

            var singleMapper = genericMapper.GetSingleMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

    /// <summary>
    /// Mapper search + mapping
    /// </summary>
    public class GenericMapAdapter : ITestableMapper
    {
        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var genericMapper = new Mapper();

            genericMapper.CreateMap<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => genericMapper.Map(src, dest);

            return action;
        }
    }
}
