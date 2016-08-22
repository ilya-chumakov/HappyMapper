using System;
using EmitMapper;

namespace OrdinaryMapper.Benchmarks
{
    public class EmitMapperAdapter : ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperAdapter();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }
}