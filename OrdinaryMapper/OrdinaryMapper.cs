using System;
using System.Collections.Generic;

namespace OrdinaryMapper
{
    public class OrdinaryMapper
    {
        public static OrdinaryMapper Instance => new OrdinaryMapper();

        public static Dictionary<string, Map> Cache { get; set; }

        public OrdinaryMapper()
        {
            Cache = new Dictionary<string, Map>();
        }

        public void Map<TSrc, TDest>(TSrc src, TDest dest)
        {
            //string key = MapContext.GetKey(sr)
            var context = MapContext.Create<TSrc, TDest>();

            Map map = null;
            Cache.TryGetValue(context.Key, out map);

            if (map == null)
                throw new OrdinaryMapperException(
                    $"Missing mapping: {context.SrcType.FullName} -> {context.DestType.FullName}. Did you forget to call CreateMap method?");

            var action = map.Method as Action<TSrc, TDest>;

            if (action == null) throw new OrdinaryMapperException("Broken cache.");

             action(src, dest);
        }

        public void CreateMap<TSrc, TDest>()
        {
            var context = MapContext.Create<TSrc, TDest>();

            Map map = null;
            Cache.TryGetValue(context.Key, out map);

            if (map == null)
            {
                var method = CreateMapMethod<TSrc, TDest>(context);
                map = new Map(method);
                Cache.Add(context.Key, map);
            }
        }

        protected Action<TSrc, TDest> CreateMapMethod<TSrc, TDest>(MapContext context)
        {
            string text = MapperTextBuilder.CreateText(context);

            var type = MapperTypeBuilder.CreateMapperType(text, context);

            return (Action<TSrc, TDest>)
                Delegate.CreateDelegate(typeof(Action<TSrc, TDest>), type, context.MapperMethodName);
        }
    }
}
