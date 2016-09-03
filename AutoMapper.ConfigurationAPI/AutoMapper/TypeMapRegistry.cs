namespace AutoMapper
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Configuration;

    public class TypeMapRegistry
    {
        public IDictionary<TypePair, TypeMap> TypeMapsDictionary { get; } = new Dictionary<TypePair, TypeMap>();

        public IEnumerable<TypeMap> TypeMaps => TypeMapsDictionary.Values;

        public void RegisterTypeMap(TypeMap typeMap) => TypeMapsDictionary[typeMap.TypePair] = typeMap;

        public TypeMap GetTypeMap(TypePair typePair) => TypeMapsDictionary.GetOrDefault(typePair);
    }
}