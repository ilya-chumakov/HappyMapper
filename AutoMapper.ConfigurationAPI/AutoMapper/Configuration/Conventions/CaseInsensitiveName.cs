using System;
using System.Linq;
using System.Reflection;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Configuration.Conventions
{
    public class CaseInsensitiveName : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            return
                getTypeInfoMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(mi => string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}