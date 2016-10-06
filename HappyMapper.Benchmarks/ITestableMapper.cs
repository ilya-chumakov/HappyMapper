using System;

namespace HappyMapper.Benchmarks
{
    public interface ITestableMapper
    {
        Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>()
            where TSrc : class, new() where TDest : class, new();
    }
}