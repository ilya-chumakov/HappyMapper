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
            //int key = MapContext.GetKey(typeof (TSrc), typeof (TDest));
            ////var context = MapContext.Create<TSrc, TDest>();

            object map = null;

            var k2 = new MapperKey(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(k2, out map);

            if (map == null)
                throw new OrdinaryMapperException(
                    "Missing mapping"
                    //$"Missing mapping: {context.SrcType.FullName} -> {context.DestType.FullName}. Did you forget to call CreateMap method?"
                    );

            return map as SingleMapper<TSrc, TDest>;
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            //int key = MapContext.GetKey(typeof (TSrc), typeof (TDest));
            ////var context = MapContext.Create<TSrc, TDest>();

            object map = null;

            var k2 = new MapperKey(typeof(TSrc), typeof(TDest), null);
            Cache.TryGetValue(k2, out map);

            //if (map == null)
            //    throw new OrdinaryMapperException(
            //        "Missing mapping"
            //        //$"Missing mapping: {context.SrcType.FullName} -> {context.DestType.FullName}. Did you forget to call CreateMap method?"
            //        );

            var mapper = map as SingleMapper<TSrc, TDest>;

            if (mapper == null) throw new OrdinaryMapperException("Broken cache.");

            mapper.Map(src, dest);
        }

        public SingleMapper<TSrc, TDest> CreateMap<TSrc, TDest>()
        {
            var context = MapContext.Create<TSrc, TDest>();
            var key = new MapperKey(typeof(TSrc), typeof(TDest), null);

            object map = null;
            Cache.TryGetValue(key, out map);

            if (map == null)
            {
                var method = CreateMapMethod<TSrc, TDest>(context);
                Method = method;

                //map = new Map(method);
                map = new SingleMapper<TSrc, TDest>(method);

                //Cache.Add(context.Key, map);
                Cache.Add(key, map);
            }

            return map as SingleMapper<TSrc, TDest>;
        }

        public object Method { get; set; }

        protected Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>(MapContext context)
        {
            string text = MapperTextBuilder.CreateText(context);

            var type = MapperTypeBuilder.CreateMapperType(text, context);

            return (Action<TSrc, TDest>)
                Delegate.CreateDelegate(typeof(Action<TSrc, TDest>), type, context.MapperMethodName);
        }

    }

    public class MapperKey
    {
        Type _TypeFrom;
        Type _TypeTo;
        string _mapperName;
        int _hash;

        public MapperKey(Type TypeFrom, Type TypeTo, string mapperName)
        {
            _TypeFrom = TypeFrom;
            _TypeTo = TypeTo;
            _mapperName = mapperName;
            _hash = TypeFrom.GetHashCode() + TypeTo.GetHashCode() + (mapperName == null ? 0 : mapperName.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var rhs = (MapperKey)obj;
            return _hash == rhs._hash && _TypeFrom == rhs._TypeFrom && _TypeTo == rhs._TypeTo && _mapperName == rhs._mapperName;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}
