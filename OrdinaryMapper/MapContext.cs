using System;

namespace OrdinaryMapper
{
    public class MapContext
    {
        public const string NamespaceName = "RoslynMappers";
        public const string MapperClassName = "Mapper";
        public string MapperMethodName { get; }
        public string MapperClassFullName { get; }

        public Type SrcType { get; }
        public Type DestType { get; }

        public MapContext(Type srcType, Type destType)
        {
            SrcType = srcType;
            DestType = destType;

            MapperMethodName = $"{NamingTools.ToAlphanumericOnly(SrcType.FullName)}_{NamingTools.ToAlphanumericOnly(DestType.FullName)}";
            MapperClassFullName = $"{NamespaceName}.{MapperClassName}";
        }

        public static int GetKey(Type srcType, Type destType)
        {
            return srcType.GetHashCode() + destType.GetHashCode();
        }

        public static MapContext Create<TSrc, TDest>()
        {
            return new MapContext(typeof(TSrc), typeof(TDest));
        }
    }
}