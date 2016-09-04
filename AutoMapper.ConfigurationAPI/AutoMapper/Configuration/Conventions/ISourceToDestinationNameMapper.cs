using System;
using System.Reflection;

namespace AutoMapper.ConfigurationAPI.Configuration.Conventions
{
    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch);
    }
}