using System;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace HappyMapper.Compilation
{
    [Obsolete]
    public static class PropertyMapFactory
    {
        public static PropertyMap CreateFake(Type srcType, Type destType, string srcPropertyName, string destPropertyName)
        {
            PropertyMap propertyMap = new PropertyMap(destType.GetProperty(destPropertyName), null);

            propertyMap.ChainMembers(new[] { srcType.GetProperty(srcPropertyName) });

            return propertyMap;
        }

        public static PropertyMap CreateReal(Type srcType, Type destType, string srcPropertyName, string destPropertyName)
        {
            return CreateReal(srcType, destType, srcPropertyName, destPropertyName, new MapperConfigurationExpression());
        }

        public static PropertyMap CreateReal(Type srcType, Type destType, 
            string srcPropertyName, string destPropertyName, MapperConfigurationExpression options)
        {
            var factory = new TypeMapFactory();

            var typeMap = factory.CreateTypeMap(srcType, destType, options);

            PropertyMap propertyMap = new PropertyMap(destType.GetProperty(destPropertyName), typeMap);

            propertyMap.ChainMembers(new[] { srcType.GetProperty(srcPropertyName) });

            return propertyMap;
        }
    }
}