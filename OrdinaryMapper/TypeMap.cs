using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OrdinaryMapper
{
    [DebuggerDisplay("{SrcType.Name} -> {DestType.Name}")]
    public class TypeMap
    {
        public Type SrcType { get; }
        public Type DestType { get; }
        public Type MapDelegateType { get; }
        public TypePair TypePair { get; }

        public TypeMap(TypePair typePair, Type mapDelegateType)
        {
            TypePair = typePair;
            SrcType = typePair.SrcType;
            DestType = typePair.DestType;
            MapDelegateType = mapDelegateType;
        }

        private readonly ConcurrentBag<PropertyMap> _propertyMaps = new ConcurrentBag<PropertyMap>();
        private bool _sealed;

        public PropertyMap FindOrCreatePropertyMapFor(MemberInfo destinationProperty)
        {
            var propertyMap = GetExistingPropertyMapFor(destinationProperty);

            if (propertyMap != null) return propertyMap;

            propertyMap = new PropertyMap(destinationProperty, this);

            _propertyMaps.Add(propertyMap);

            return propertyMap;
        }


        public PropertyMap GetExistingPropertyMapFor(MemberInfo destinationProperty)
        {
            if (!destinationProperty.DeclaringType.IsAssignableFrom(DestType))
                return null;

            var propertyMap =
                _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            return propertyMap;
        }

        public ConcurrentBag<PropertyMap> PropertyMaps
        {
            get { return _propertyMaps; }
        }

        public void AddPropertyMap(MemberInfo destProperty, IEnumerable<MemberInfo> resolvers)
        {
            var propertyMap = new PropertyMap(destProperty, this);

            propertyMap.ChainMembers(resolvers);

            _propertyMaps.Add(propertyMap);
        }

        public void Seal(TypeMapRegistry typeMapRegistry, MapperConfiguration configurationProvider)
        {
            if (_sealed)
            {
                return;
            }
            _sealed = true;

            //foreach (var inheritedTypeMap in _inheritedTypeMaps)
            //{
            //    ApplyInheritedTypeMap(inheritedTypeMap);
            //}

            //_orderedPropertyMaps =
            //    _propertyMaps
            //        .Union(_inheritedMaps)
            //        .OrderBy(map => map.MappingOrder).ToArray();

            //MapExpression = new TypeMapPlanBuilder(configurationProvider, typeMapRegistry, this).CreateMapperLambda();
        }

    }
}