using System;
using System.Collections.Generic;
using AutoMapper.ConfigurationAPI;
using OrdinaryMapper.Compilation;
using OrdinaryMapper.Text;

namespace OrdinaryMapper
{
    public class HappyMapper
    {
        public Dictionary<TypePair, CompiledDelegate> DelegateCache { get; private set; }
        public Dictionary<TypePair, TypeMap> TypeMaps { get; } = new Dictionary<TypePair, TypeMap>();
        public MapperNameConvention Convention { get; set; } = NameConventionsStorage.Mapper;

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

            var mapMethod = @delegate?.Single as Action<TSrc, TDest>;

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
    }
}
