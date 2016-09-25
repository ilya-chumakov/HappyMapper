using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper.Saved
{
    public static class AmcApiEx
    {

        public static void AddIfNotExist(this IDictionary<TypePair, TypeMap> origin, TypeMap typeMap)
        {
            origin.AddIfNotExist(typeMap.TypePair, typeMap);
        }
    }

    internal static class TypeExtensions
    {
        public static bool IsImplicitCastableFrom(this Type dest, Type src)
        {
            return ImplicitCastChecker.CanCast(src, dest);
        }

        public static bool IsExplicitCastableFrom(this Type dest, Type src)
        {
            return ExplicitCastChecker.CanCast(src, dest);
        }
    }

    public static class DictionaryEx
    {
        public static void AddIfNotExist<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = value;
            }
        }

        public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue>(this IDictionary<TKey, TValue> origin)
        {
            return origin.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}