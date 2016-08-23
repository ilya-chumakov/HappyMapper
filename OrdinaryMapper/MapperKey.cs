using System;

namespace OrdinaryMapper
{
    public class MapperKey
    {
        public Type SrcType { get; }
        public Type DestType { get; }
        string _mapperName;
        int _hash;

        public MapperKey(Type srcType, Type destType, string mapperName = null)
        {
            SrcType = srcType;
            DestType = destType;
            _mapperName = mapperName;
            _hash = srcType.GetHashCode() + destType.GetHashCode() + (mapperName == null ? 0 : mapperName.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var rhs = (MapperKey)obj;
            return _hash == rhs._hash && SrcType == rhs.SrcType && DestType == rhs.DestType && _mapperName == rhs._mapperName;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}