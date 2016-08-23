using System;

namespace OrdinaryMapper
{
    public class MapContext
    {
        public const string NamespaceName = "RoslynMappers";
        public const string MapperClassName = "Mapper";
        public string MapperMethodName => $"{NamingTools.Clean(SrcType.FullName)}_{NamingTools.Clean(DestType.FullName)}";
        public string MapperClassFullName => $"{NamespaceName}.{MapperClassName}";

        public Type SrcType { get; }
        public Type DestType { get; }

        public MapContext(Type srcType, Type destType)
        {
            SrcType = srcType;
            DestType = destType;
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