using System.Collections.Generic;
using System.Reflection;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public interface IStorageBuilder
    {
        string BuildCode();

        void InitStorage(Assembly assembly);
    }
}