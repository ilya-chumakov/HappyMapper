using System;

namespace OrdinaryMapper
{
    public class MapContext
    {
        public MapContext(Type srcType, Type destType)
        {
            SrcType = srcType;
            DestType = destType;
            Key = GetKey(SrcType, DestType);
        }

        public static string GetKey(Type srcType, Type destType)
        {
            return srcType.FullName + "-" + destType.FullName;
        }

        public static MapContext Create<TSrc, TDest>()
        {
            return new MapContext(typeof(TSrc), typeof(TDest));
        }

        public Type SrcType { get; private set; }
        public Type DestType { get; private set; }

        public string MapperMethodName => $"{NamingTools.Clean(SrcType.FullName)}_{NamingTools.Clean(DestType.FullName)}";
        public string NamespaceName => "RoslynMappers";
        public string MapperClassName => "Mapper";
        public string MapperClassFullName => $"{NamespaceName}.{MapperClassName}";
        public string Key { get; private set; }
    }
}