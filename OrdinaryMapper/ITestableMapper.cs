using System;

namespace OrdinaryMapper
{
    public interface ITestableMapper
    {
        Action<TInput, TOutput> CreateMapMethod<TInput, TOutput>();
    }
}