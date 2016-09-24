using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using OrdinaryMapper.Obsolete;

namespace OrdinaryMapper
{
    public class Mapper
    {
        public static Mapper Instance { get; } = new Mapper();

        public Dictionary<TypePair, object> DelegateCache { get; private set; }
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();
        public MapperNameConvention Convention { get; set; } = NameConventions.Mapper;

        public Mapper()
        {
            DelegateCache = new Dictionary<TypePair, object>();
        }

        public Mapper(Dictionary<TypePair, object> delegates)
        {
            DelegateCache = delegates;
        }

        public SingleMapper<TSrc, TDest> GetSingleMapper<TSrc, TDest>()
        {
            object @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest), null);
            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate as Action<TSrc, TDest>;

            if (mapMethod == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            var mapper = new SingleMapper<TSrc, TDest>(mapMethod);

            return mapper;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            object @delegate;

            var key = new TypePair(typeof(TSrc), typeof(TDest));
            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate as Action<TSrc, TDest>;

            if (mapMethod == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            mapMethod(src, dest);
        }

        public void CreateMap<TSrc, TDest>()
        {
            var typePair = new TypePair(typeof(TSrc), typeof(TDest));

            TypeMap map;
            TypeMaps.TryGetValue(typePair, out map);

            if (map == null) TypeMaps.Add(typePair, new TypeMap(typePair, typeof(Action<TSrc, TDest>)));
        }

        public void Compile()
        {
            var texts = new List<string>();
            var types = new HashSet<Type>();

            foreach (var kvp in TypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var context = new MapContext(typePair.SrcType, typePair.DestType);

                string text = MapperTextBuilder.CreateText(context);

                texts.Add(text);

                types.Add(typePair.SrcType);
                types.Add(typePair.DestType);
            }

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(texts.ToArray(), types);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            foreach (var kvp in TypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var context = new MapContext(typePair.SrcType, typePair.DestType);

                var type = assembly.GetType($"{Convention.ClassFullName}");

                var @delegate = Delegate.CreateDelegate(map.MapDelegateType, type, Convention.GetMapperMethodName(map));

                DelegateCache.Add(typePair, @delegate);
            }
        }
    }
}
