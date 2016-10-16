using System;
using System.Reflection;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Configuration.Conventions
{
    public abstract class SourceToDestinationMapperAttribute : Attribute
    {
        public abstract bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, Type destMemberType, string nameToSearch);
    }
}