using System;

#if NET45
namespace AutoMapper.ConfigurationAPI.Execution
{
    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }
}
#endif