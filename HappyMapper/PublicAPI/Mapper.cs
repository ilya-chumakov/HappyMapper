using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Compilation;
using HappyMapper.Text;

namespace HappyMapper
{
    public class Mapper
    {
        public Dictionary<TypePair, CompiledDelegate> DelegateCache { get; private set; }
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();
        public MapperNameConvention Convention { get; set; } = NameConventionsStorage.Mapper;

        [Obsolete("Call HappyConfig.CompileMapper to create a mapper instance.")]
        public Mapper()
        {
            DelegateCache = new Dictionary<TypePair, CompiledDelegate>();
        }

        internal Mapper(Dictionary<TypePair, CompiledDelegate> delegates)
        {
            DelegateCache = delegates;
        }

        public SingleMapper<TSrc, TDest> GetSingleMapper<TSrc, TDest>()
        {
            CompiledDelegate @delegate = null;

            var key = new TypePair(typeof(TSrc), typeof(TDest));
            DelegateCache.TryGetValue(key, out @delegate);

            var mapMethod = @delegate?.Single as Func<TSrc, TDest, TDest>;

            if (mapMethod == null) throw new HappyMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            var mapper = new SingleMapper<TSrc, TDest>(mapMethod);

            return mapper;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
            where TSrc : class , new() where TDest : class , new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dest == null) throw new ArgumentNullException(nameof(dest));

            var key = new TypePair(typeof(TSrc), typeof(TDest));

            var mapMethod = GetMapMethod<TSrc, TDest>(key, @delegate => @delegate?.Single);

            mapMethod(src, dest);
        }

        public void MapCollection<TSrc, TDest>(ICollection<TSrc> src, ICollection<TDest> dest)
            where TSrc : class, new() where TDest : class, new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dest == null) throw new ArgumentNullException(nameof(dest));

            var key = new TypePair(typeof(TSrc), typeof(TDest));

            var mapMethod = GetMapMethod<ICollection<TSrc>, ICollection<TDest>>(key, @delegate => @delegate?.Collection);

            mapMethod(src, dest);
        }

        private Func<TSrc, TDest, TDest> GetMapMethod<TSrc, TDest>(TypePair key, Func<CompiledDelegate, object> propertyAccessor)
        {
            CompiledDelegate @delegate = null;

            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = propertyAccessor(@delegate) as Func<TSrc, TDest, TDest>;

            if (mapMethod == null)
                throw new HappyMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            return mapMethod;
        }

        public TDest Map<TDest>(object src)
            where TDest : class, new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var key = new TypePair(src.GetType(), typeof(TDest));

            CompiledDelegate @delegate = null;

            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate?.SingleOneArg as Func<object, TDest>;

            if (mapMethod == null)
                throw new HappyMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            return mapMethod(src);
        }

        public TDest MapCollectionWithCreate<TSrc, TDest>(TSrc src)
            where TSrc : class, IEnumerable, new() 
            where TDest : class, IEnumerable, new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var srcType = GetCollectionGenericTypeArgument<TSrc>();
            var destType = GetCollectionGenericTypeArgument<TDest>();

            var key = new TypePair(srcType, destType);

            CompiledDelegate @delegate = null;

            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = @delegate?.CollectionUntyped as Action<object, object>;

            if (mapMethod == null)
                throw new HappyMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            var dest = new TDest();

            mapMethod(src, dest);

            return dest;
        }

        private Type GetCollectionGenericTypeArgument<TCollection>()
        {
            var collectionType = typeof (TCollection);

            if (collectionType.GenericTypeArguments.Count() != 1) 
                throw new NotSupportedException($"The type {collectionType.FullName} is not supported.");

            var type = collectionType.GenericTypeArguments[0];

            return type;
        }
    }
}
