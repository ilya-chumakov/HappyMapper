using System;
using System.Collections;
using System.Diagnostics;

namespace OrdinaryMapper
{
    [DebuggerDisplay("{SrcType.Name} -> {DestType.Name}")]
    public class MapperKey : IEquatable<MapperKey>
    {
        public Type SrcType { get; }
        public Type DestType { get; }
        readonly string _mapperName;
        readonly int _hash;

        public MapperKey(Type srcType, Type destType, string mapperName = null)
        {
            SrcType = srcType;
            DestType = destType;
            _mapperName = mapperName;
            _hash = srcType.GetHashCode() + destType.GetHashCode() + (mapperName == null ? 0 : mapperName.GetHashCode());
        }

        public bool Equals(MapperKey other)
        {
            return _hash == other._hash
                && SrcType == other.SrcType
                && DestType == other.DestType
                && _mapperName == other._mapperName;
        }

        public override bool Equals(object obj)
        {
            var other = (MapperKey)obj;

            return _hash == other._hash
                && SrcType == other.SrcType
                && DestType == other.DestType
                && _mapperName == other._mapperName;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}