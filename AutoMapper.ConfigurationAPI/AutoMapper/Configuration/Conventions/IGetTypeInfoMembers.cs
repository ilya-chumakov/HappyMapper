using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.ConfigurationAPI.Configuration.Conventions
{
    public interface IGetTypeInfoMembers
    {
        IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo);
        IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate);
    }
}