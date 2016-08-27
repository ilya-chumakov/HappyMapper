using System.Collections.Generic;

namespace OrdinaryMapper
{
    public static class DictionaryEx
    {
        public static void AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.TryGetValue(key, out value))
            {
                dict[key] = value;
            }
        }
    }
}