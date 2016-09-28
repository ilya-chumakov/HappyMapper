using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using Microsoft.CodeAnalysis.CSharp;

namespace OrdinaryMapper
{
    public class HappyMapper
    {
        public static Mapper Instance { get; } = new Mapper();

        public Dictionary<TypePair, CompiledDelegate> DelegateCache { get; private set; }
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();
        public MapperNameConvention Convention { get; set; } = NameConventions.Mapper;

        public HappyMapper()
        {
            DelegateCache = new Dictionary<TypePair, CompiledDelegate>();
        }

        public HappyMapper(Dictionary<TypePair, CompiledDelegate> delegates)
        {
            DelegateCache = delegates;
        }

        public SingleMapper<TSrc, TDest> GetSingleMapper<TSrc, TDest>()
        {
            CompiledDelegate @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest));
            DelegateCache.TryGetValue(key, out @delegate);

            var mapMethod = @delegate.Single as Action<TSrc, TDest>;

            if (mapMethod == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            var mapper = new SingleMapper<TSrc, TDest>(mapMethod);

            return mapper;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            CompiledDelegate @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest));
            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate.Single as Action<TSrc, TDest>;

            if (mapMethod == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            mapMethod(src, dest);
        }

        public void MapCollection<TSrc, TDest>(ICollection<TSrc> src, ICollection<TDest> dest)
        {
            CompiledDelegate @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest));
            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate.Collection as Action<ICollection<TSrc>, ICollection<TDest>>;

            if (mapMethod == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            mapMethod(src, dest);
        }

        public void CreateMap<TSrc, TDest>()
        {
            var typePair = new TypePair(typeof(TSrc), typeof(TDest));

            TypeMap map;
            TypeMaps.TryGetValue(typePair, out map);

            //if (map == null) TypeMaps.Add(typePair, new TypeMap(typePair, typeof(Action<TSrc, TDest>)));
            if (map == null)
            {
                var mapDelegateType = typeof(Action<TSrc, TDest>);

                //TypeMaps.Add(typePair, new TypeMap(typePair, mapDelegateType));

                var tm = TypeMapFactory.CreateTypeMapEx(
                    typePair, null, MemberList.Destination, mapDelegateType);

                TypeMaps.Add(typePair, tm);
            }
        }

        public TypeMapFactory TypeMapFactory { get; set; }
        
        //public void Compile()
        //{
        //    var texts = new List<string>();
        //    var types = new HashSet<Type>();

        //    foreach (var kvp in TypeMaps)
        //    {
        //        TypePair typePair = kvp.Key;
        //        TypeMap map = kvp.Value;

        //        var context = new MapContext(typePair.SourceType, typePair.DestinationType);

        //        string text = MapperTextBuilder.CreateText(context);

        //        texts.Add(text);

        //        types.Add(typePair.SourceType);
        //        types.Add(typePair.DestinationType);
        //    }

        //    CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(texts.ToArray(), types);

        //    Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

        //    foreach (var kvp in TypeMaps)
        //    {
        //        TypePair typePair = kvp.Key;
        //        TypeMap map = kvp.Value;

        //        var context = new MapContext(typePair.SourceType, typePair.DestinationType);

        //        var type = assembly.GetType($"{Convention.ClassFullName}");

        //        var @delegate = Delegate.CreateDelegate(map.MapDelegateType, type, Convention.GetMapperMethodName(map));

        //        DelegateCache.Add(typePair, @delegate);
        //    }
        //}
    }
}
