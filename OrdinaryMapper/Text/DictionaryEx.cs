using System.Collections.Generic;
using System.Linq;

namespace OrdinaryMapper
{
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
        public static void AddIfNotExist(this IDictionary<TypePair, TypeMap> origin, TypeMap typeMap)
        {
            origin.AddIfNotExist(typeMap.TypePair, typeMap);
        }
    }
}