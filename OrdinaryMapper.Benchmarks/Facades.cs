using System;
using EmitMapper;

namespace OrdinaryMapper.Benchmarks
{
    public class EmitMapperFacade : ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperFacade();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }

    public class OrdinaryMapperFacade : ITestableMapper
    {
        public static ITestableMapper Instance => new OrdinaryMapperFacade();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            OrdinaryMapper.Instance.CreateMap<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => OrdinaryMapper.Instance.Map(src, dest);

            return action;
        }
    }
}
