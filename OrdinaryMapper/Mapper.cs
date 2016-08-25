using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace OrdinaryMapper
{
    public class Mapper
    {
        public static Mapper Instance { get; } = new Mapper();

        public Dictionary<TypePair, object> DelegateCache { get; } = new Dictionary<TypePair, object>();
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();

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
            object @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest), null);
            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate as Action<TSrc, TDest>;

            if (@delegate == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            var mapper = new SingleMapper<TSrc, TDest>(mapMethod);

            if (mapper == null) throw new OrdinaryMapperException("Broken cache.");

            mapper.Map(src, dest);
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

                var type = assembly.GetType($"{context.MapperClassFullName}");

                var @delegate = Delegate.CreateDelegate(map.MapDelegateType, type, context.MapperMethodName);

                DelegateCache.Add(typePair, @delegate);
            }
        }

        public SingleMapper<TSrc, TDest> CreateMap_OLD<TSrc, TDest>()
        {
            var key = new TypePair(typeof(TSrc), typeof(TDest), null);

            object map = null;
            DelegateCache.TryGetValue(key, out map);

            if (map == null)
            {
                var method = CreateMapMethod<TSrc, TDest>(key);

                map = new SingleMapper<TSrc, TDest>(method);

                DelegateCache.Add(key, map);
            }

            return map as SingleMapper<TSrc, TDest>;
        }

        protected Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>(Type type)
        {
            var context = MapContext.Create<TSrc, TDest>();

            return (Action<TSrc, TDest>)
                Delegate.CreateDelegate(typeof(Action<TSrc, TDest>), type, context.MapperMethodName);
        }

        protected Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>(TypePair key)
        {
            var context = new MapContext(key.SrcType, key.DestType);

            string text = MapperTextBuilder.CreateText(context);

            var type = MapperTypeBuilder.CreateMapperType(text, context);

            return (Action<TSrc, TDest>)
                Delegate.CreateDelegate(typeof(Action<TSrc, TDest>), type, context.MapperMethodName);
        }
    }
}
