using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrdinaryMapper.Obsolete
{
    public class TypeMapFactory
    {
        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, MapperConfigurationExpression options)
        {
            //TODO: cache typedetails

            var sourceTypeInfo = new TypeDetails(sourceType, options);
            var destTypeInfo = new TypeDetails(destinationType, options);
            //var sourceTypeInfo = options.CreateTypeDetails(sourceType);
            //var destTypeInfo = options.CreateTypeDetails(destinationType);

            var typeMap = new TypeMap(new TypePair(sourceType, destinationType), null); //TODO
            //var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var resolvers = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(options, sourceTypeInfo, destProperty.DeclaringType, destProperty.GetMemberType(), destProperty.Name, resolvers))
                {
                    //как получить SOURCE?
                    typeMap.AddPropertyMap(destProperty, resolvers);
                }
            }
            //if (!destinationType.IsAbstract() && destinationType.IsClass())
            //{
            //    foreach (var destCtor in destTypeInfo.Constructors.OrderByDescending(ci => ci.GetParameters().Length))
            //    {
            //        if (MapDestinationCtorToSource(typeMap, destCtor, sourceTypeInfo, options))
            //        {
            //            break;
            //        }
            //    }
            //}
            return typeMap;
        }

        private bool MapDestinationPropertyToSource(MapperConfigurationExpression options, TypeDetails sourceTypeInfo, Type destType, Type destMemberType, string destMemberInfo, LinkedList<MemberInfo> members)
        {
            return options.MemberConfigurations.Any(_ => _.MapDestinationPropertyToSource(options, sourceTypeInfo, destType, destMemberType, destMemberInfo, members));
        }

    }
}