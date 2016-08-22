using System;
using EmitMapper;

namespace OrdinaryMapper.Benchmarks
{
    public class EmitMapperSingle : ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperSingle();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TInput, TOutput>();

            Action<TInput, TOutput> action = (src, dest) => mapper.Map(src, dest);

            return action;
        }
    }

    public class EmitMapperCached: ITestableMapper
    {
        public static ITestableMapper Instance => new EmitMapperCached();

        public Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>()
        {
            Action<TInput, TOutput> action = (src, dest) =>
            {
                var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TInput, TOutput>();
                mapper.Map(src, dest);
            };

            return action;
        }
    }
}