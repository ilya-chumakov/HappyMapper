using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.ConfigurationAPI.Configuration.Conventions
{
    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfiguration parent);
    }
}