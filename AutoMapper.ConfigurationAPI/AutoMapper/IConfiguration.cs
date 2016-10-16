using System;
using System.Collections.Generic;

namespace HappyMapper.AutoMapper.ConfigurationAPI
{
    public interface IConfiguration : IProfileConfiguration
    {
        Func<Type, object> ServiceCtor { get; }
        IEnumerable<Profile> Profiles { get; }
    }
}