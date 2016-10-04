using System;
using System.Collections.Generic;

namespace OrdinaryMapper
{
    public class LegacyMapContext
    {
        public Type SrcType { get; }
        public Type DestType { get; }

        public LegacyMapContext(Type srcType, Type destType)
        {
            SrcType = srcType;
            DestType = destType;
        }

        public static int GetKey(Type srcType, Type destType)
        {
            return srcType.GetHashCode() + destType.GetHashCode();
        }

        public static LegacyMapContext Create<TSrc, TDest>()
        {
            return new LegacyMapContext(typeof(TSrc), typeof(TDest));
        }

        public HashSet<Type> ToHashSet()
        {
            var types = new HashSet<Type>();

            types.Add(SrcType);
            types.Add(DestType);

            return types;
        }
    }
}