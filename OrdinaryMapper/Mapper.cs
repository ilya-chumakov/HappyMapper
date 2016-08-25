using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace OrdinaryMapper
{
    public class Mapper
    {
        public static Mapper Instance { get; } = new Mapper();

        public Dictionary<TypePair, object> Cache { get; } = new Dictionary<TypePair, object>();
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();

        public SingleMapper<TSrc, TDest> GetSingleMapper<TSrc, TDest>()
        {
            object mapperType = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(key, out mapperType);

            if (mapperType == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            var method = CreateMapMethod<TSrc, TDest>(mapperType as Type);

            var mapper = new SingleMapper<TSrc, TDest>(method);

            return mapper;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            object mapperType = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(key, out mapperType);

            if (mapperType == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            var method = CreateMapMethod<TSrc, TDest>(mapperType as Type);

            var mapper = new SingleMapper<TSrc, TDest>(method);

            if (mapper == null) throw new OrdinaryMapperException("Broken cache.");

            mapper.Map(src, dest);
        }

        public void CreateMap<TSrc, TDest>()
        {
            var typePair = new TypePair(typeof(TSrc), typeof(TDest));

            TypeMap map;
            TypeMaps.TryGetValue(typePair, out map);

            if (map == null) TypeMaps.Add(typePair, new TypeMap(typePair));
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

                //map = new SingleMapper<TSrc, TDest>(method);

                Cache.Add(typePair, type);
            }
        }

        public SingleMapper<TSrc, TDest> CreateMap_OLD<TSrc, TDest>()
        {
            var key = new TypePair(typeof(TSrc), typeof(TDest), null);

            object map = null;
            Cache.TryGetValue(key, out map);

            if (map == null)
            {
                var method = CreateMapMethod<TSrc, TDest>(key);

                map = new SingleMapper<TSrc, TDest>(method);

                Cache.Add(key, map);
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
