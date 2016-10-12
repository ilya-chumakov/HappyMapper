using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;
using HappyMapper.Text;

namespace HappyMapper
{
    public class Mapper
    {
        internal Dictionary<TypePair, CompiledDelegate> DelegateCache { get; private set; }
        internal Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();
        internal MapperNameConvention Convention { get; set; } = NameConventionsStorage.Mapper;

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

        public TDest Map<TSrc, TDest>(TSrc src, TDest dest)
            where TSrc : class , new() where TDest : class , new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dest == null) throw new ArgumentNullException(nameof(dest));

            var srcType = src.GetType();

            if (!srcType.IsCollectionType()) return MapTypedSingle<TSrc, TDest>(src, dest);

            var destType = typeof(TDest);

            if (destType.IsCollectionType())
            {
                return MapUntypedCollection(src, dest, srcType, destType);
            }

            throw new NotSupportedException("These types are not supported!");
        }

        public TDest Map<TDest>(object src)
            where TDest : class, new()
        {
            return MapUntyped<TDest>(src);
        }

        public TDest Map<TSrc, TDest>(TSrc src)
            where TSrc : class, new()
            where TDest : class, new()
        {
            return MapUntyped<TDest>(src);
        }

        private TDest MapTypedSingle<TSrc, TDest>(TSrc src, TDest dest) where TSrc : class, new() where TDest : class, new()
        {
            var key = new TypePair(typeof (TSrc), typeof (TDest));

            var mapMethod = GetMapMethod(key, @delegate => @delegate?.Single)
                as Func<TSrc, TDest, TDest>;

            mapMethod(src, dest);

            return dest;
        }

        private TDest MapUntyped<TDest>(object src) where TDest : class, new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var srcType = src.GetType();

            if (!srcType.IsCollectionType()) return MapUntypedSingle<TDest>(src);

            var destType = typeof(TDest);

            if (destType.IsCollectionType())
            {
                var dest = new TDest();

                return MapUntypedCollection(src, dest, srcType, destType);
            }

            throw new NotSupportedException("These types are not supported!");
        }

        private TDest MapUntypedSingle<TDest>(object src)
            where TDest : class, new()
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var key = new TypePair(src.GetType(), typeof(TDest));

            var mapMethod = GetMapMethod(key, @delegate => @delegate?.SingleOneArg)
                as Func<object, TDest>;

            return mapMethod(src);
        }

        private TDest MapUntypedCollection<TSrc, TDest>(TSrc src, TDest dest, Type srcType, Type destType)
            where TSrc : class, new() where TDest : class, new()
        {
            var srcItemType = GetCollectionGenericTypeArgument(srcType);
            var destItemType = GetCollectionGenericTypeArgument(destType);

            var key = new TypePair(srcItemType, destItemType);

            var mapMethod = GetMapMethod(key, @delegate => @delegate?.CollectionUntyped)
                as Action<object, object>;

            mapMethod(src, dest);

            return dest;
        }

        private object GetMapMethod(TypePair key, Func<CompiledDelegate, object> propertyAccessor)
        {
            CompiledDelegate @delegate = null;

            DelegateCache.TryGetValue(key, out @delegate);
            var mapMethod = propertyAccessor(@delegate);

            if (mapMethod == null)
                throw new HappyMapperException(ErrorMessages.MissingMapping(key.SourceType, key.DestinationType));

            return mapMethod;
        }

        private Type GetCollectionGenericTypeArgument<TCollection>()
        {
            var collectionType = typeof (TCollection);

            return GetCollectionGenericTypeArgument(collectionType);
        }

        private static Type GetCollectionGenericTypeArgument(Type collectionType)
        {
            if (collectionType.GenericTypeArguments.Count() != 1)
                throw new NotSupportedException($"The type {collectionType.FullName} is not supported.");

            return collectionType.GenericTypeArguments[0];
        }

        //public void MapCollection<TSrc, TDest>(ICollection<TSrc> src, ICollection<TDest> dest)
        //    where TSrc : class, new() where TDest : class, new()
        //{
        //    if (src == null) throw new ArgumentNullException(nameof(src));
        //    if (dest == null) throw new ArgumentNullException(nameof(dest));


        //    //var key = new TypePair(typeof(TSrc), typeof(TDest));

        //    //var mapMethod = GetMapMethod(key, @delegate => @delegate?.Collection)
        //    //     as Func<ICollection<TSrc>, ICollection<TDest>, ICollection<TDest>>;

        //    //mapMethod(src, dest);
        //}
    }
}
