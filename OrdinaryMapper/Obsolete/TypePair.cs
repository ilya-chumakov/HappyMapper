﻿using System;
using System.Diagnostics;

namespace OrdinaryMapper.Obsolete
{
    [DebuggerDisplay("{SrcType.Name} -> {DestType.Name}")]
    public class TypePair : IEquatable<TypePair>
    {
        public Type SrcType { get; }
        public Type DestType { get; }
        readonly string _mapperName;
        readonly int _hash;

        public TypePair(Type srcType, Type destType, string mapperName = null)
        {
            SrcType = srcType;
            DestType = destType;
            _mapperName = mapperName;
            _hash = srcType.GetHashCode() + destType.GetHashCode() + (mapperName == null ? 0 : mapperName.GetHashCode());
        }

        public bool Equals(TypePair other)
        {
            return _hash == other._hash
                && SrcType == other.SrcType
                && DestType == other.DestType
                && _mapperName == other._mapperName;
        }

        public override bool Equals(object obj)
        {
            var other = (TypePair)obj;

            return _hash == other._hash
                && SrcType == other.SrcType
                && DestType == other.DestType
                && _mapperName == other._mapperName;
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override string ToString()
        {
            return $"SrcType: {SrcType.Name} -> DestType: {DestType.Name}.";
        }
    }
}