using System;

namespace OrdinaryMapper.Benchmarks
{
    public class SingleMapperAdapter : ITestableMapper
    {
        public static ITestableMapper Instance => new SingleMapperAdapter();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var singleMapper = OrdinaryMapper.Instance.CreateMap<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);
            //Action<TInput, TOutput> action = (src, dest) => OrdinaryMapper.Instance.Map(src, dest);

            return action;
        }
    }

    public class TakeSingleMapperFromCacheAdapter : ITestableMapper
    {
        public static ITestableMapper Instance => new TakeSingleMapperFromCacheAdapter();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            OrdinaryMapper.Instance.CreateMap<TInput, TOutput>();

            var singleMapper = OrdinaryMapper.Instance.GetSingleMapper<TInput, TOutput>();

            Action <TInput, TOutput> action = (src, dest) => singleMapper.Map(src, dest);

            return action;
        }
    }

    public class GenericMapAdapter : ITestableMapper
    {
        public static ITestableMapper Instance => new GenericMapAdapter();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            OrdinaryMapper.Instance.CreateMap<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => OrdinaryMapper.Instance.Map(src, dest);

            return action;
        }
    }
}
