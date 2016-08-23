using System;
using System.Collections.Generic;

namespace OrdinaryMapper
{
    public class OrdinaryMapper
    {
        public static OrdinaryMapper Instance { get; } = new OrdinaryMapper();

        public static Dictionary<MapperKey, object> Cache { get; } = new Dictionary<MapperKey, object>();

        public SingleMapper<TSrc, TDest> GetSingleMapper<TSrc, TDest>()
        {
            object map = null;

            var key = new MapperKey(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(key, out map);

            if (map == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            return map as SingleMapper<TSrc, TDest>;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            object map = null;

            var key = new MapperKey(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(key, out map);

            if (map == null) throw new OrdinaryMapperException(ErrorMessages.MissingMapping(key.SrcType, key.DestType));

            var mapper = map as SingleMapper<TSrc, TDest>;

            if (mapper == null) throw new OrdinaryMapperException("Broken cache.");

            mapper.Map(src, dest);
        }

        public SingleMapper<TSrc, TDest> CreateMap<TSrc, TDest>()
        {
            var key = new MapperKey(typeof(TSrc), typeof(TDest), null);

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

        protected Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>(MapperKey key)
        {
            var context = new MapContext(key.SrcType, key.DestType);

            string text = MapperTextBuilder.CreateText(context);

            var type = MapperTypeBuilder.CreateMapperType(text, context);

            return (Action<TSrc, TDest>)
                Delegate.CreateDelegate(typeof(Action<TSrc, TDest>), type, context.MapperMethodName);
        }
    }
}
