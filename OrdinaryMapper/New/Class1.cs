using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OrdinaryMapper.MemberMapping
{
    public interface IGetTypeInfoMembers
    {
        IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo);
        IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate);
    }

    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch);
    }

    public interface IParentSourceToDestinationNameMapper
    {
        ICollection<ISourceToDestinationNameMapper> NamedMappers { get; }
        IGetTypeInfoMembers GetMembers { get; }
        MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch);
    }

    public class CaseSensitiveName : ISourceToDestinationNameMapper
    {
        public bool MethodCaseSensitive { get; set; }

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            return
                getTypeInfoMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(
                        mi =>
                            typeof(ParameterInfo).IsAssignableFrom(destType) || !MethodCaseSensitive
                                ? string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0
                                : string.CompareOrdinal(mi.Name, nameToSearch) == 0);
        }
    }

    public class DefaultName : CaseSensitiveName
    {
    }

    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        public IGetTypeInfoMembers GetMembers { get; } = new AllMemberInfo();

        public ICollection<ISourceToDestinationNameMapper> NamedMappers { get; } 
            = new Collection<ISourceToDestinationNameMapper> { new DefaultName() };

        public MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(GetMembers, typeInfo, destType, destMemberType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }

    public class AllMemberInfo : IGetTypeInfoMembers
    {
        private readonly IList<Func<MemberInfo, bool>> _predicates = new List<Func<MemberInfo, bool>>();

        public IEnumerable<MemberInfo> GetMemberInfos(TypeDetails typeInfo)
        {
            return !_predicates.Any()
                ? typeInfo.AllMembers
                : typeInfo.AllMembers.Where(m => _predicates.All(p => p(m))).ToList();
        }

        public IGetTypeInfoMembers AddCondition(Func<MemberInfo, bool> predicate)
        {
            _predicates.Add(predicate);
            return this;
        }
    }

    public interface IMemberConfigurationCONV
    {
        IList<IChildMemberConfiguration> MemberMappers { get; }
        IMemberConfigurationCONV AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new();

        //IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
        //    where TNameMapper : ISourceToDestinationNameMapper, new();

        IParentSourceToDestinationNameMapper NameMapper { get; set; }
        bool MapDestinationPropertyToSource(
            MapperConfigurationExpression options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers);
    }

    public class MemberConfigurationConv : IMemberConfigurationCONV
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public IList<IChildMemberConfiguration> MemberMappers { get; } = new Collection<IChildMemberConfiguration>();

        public IMemberConfigurationCONV AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new()
        {
            GetOrAdd(_ => (IList)_.MemberMappers, setupAction);
            return this;
        }

        //public IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
        //    where TNameMapper : ISourceToDestinationNameMapper, new()
        //{
        //    GetOrAdd(_ => (IList)_.NameMapper.NamedMappers, setupAction);
        //    return this;
        //}

        private TMemberMapper GetOrAdd<TMemberMapper>(Func<IMemberConfigurationCONV, IList> getList, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : new()
        {
            var child = getList(this).OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new TMemberMapper();
                getList(this).Add(child);
            }
            setupAction?.Invoke(child);
            return child;
        }

        public MemberConfigurationConv()
        {
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(MapperConfigurationExpression options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers)
        {
            var foundMap = false;
            foreach (var memberMapper in MemberMappers)
            {
                foundMap = memberMapper.MapDestinationPropertyToSource(options, sourceType, destType, destMemberType, nameToSearch, resolvers, this);
                if (foundMap)
                    break;
            }
            return foundMap;
        }
    }

    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(MapperConfigurationExpression options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfigurationCONV parent);
    }

    public class DefaultMember : IChildMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public bool MapDestinationPropertyToSource(MapperConfigurationExpression options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfigurationCONV parent = null)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo);
            return matchingMemberInfo != null;
        }
    }

}