using System;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    public class MapperNameConvention
    {
        public string Namespace { get; set; }
        public string ClassShortName { get; set; }

        public string ClassFullName => $"{Namespace}.{ClassShortName}";

        public string GetMapperMethodName(Type srcType, Type destType)
        {
            return $"{NamingTools.ToAlphanumericOnly(srcType.FullName)}_{NamingTools.ToAlphanumericOnly(destType.FullName)}";
        }

        public string GetMapperMethodName(OrdinaryMapper.Obsolete.TypeMap tm)
        {
            return GetMapperMethodName(tm.SrcType, tm.DestType);
        }

        public string GetMapperMethodName(AutoMapper.ConfigurationAPI.TypeMap tm)
        {
            return GetMapperMethodName(tm.SourceType, tm.DestinationType);
        }


        public string GetUniqueMapperMethodNameWithGuid(TypePair typePair)
        {
            string guid = NamingTools.NewGuid();

            string normSrcName = NamingTools.ToAlphanumericOnly(typePair.SourceType.Name);
            string normDestName = NamingTools.ToAlphanumericOnly(typePair.DestinationType.Name);

            return $"Mapper_{normSrcName}_{normDestName}_{guid}";
        }
    }
}