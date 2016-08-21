using System;

namespace OrdinaryMapper.Benchmarks
{
    public interface ITestableMapper
    {
        Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>();
    }
}