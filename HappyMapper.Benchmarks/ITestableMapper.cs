using System;

namespace HappyMapper.Benchmarks
{
    public interface ITestableMapper
    {
        Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>();
    }
}