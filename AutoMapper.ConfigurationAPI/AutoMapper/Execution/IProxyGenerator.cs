using System;

#if NET45
namespace HappyMapper.AutoMapper.ConfigurationAPI.Execution
{
    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }
}
#endif