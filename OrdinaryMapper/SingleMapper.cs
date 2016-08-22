using System;

namespace OrdinaryMapper
{
    public class SingleMapper<TSrc, TDest> //: Map
    {
        public SingleMapper(Action<TSrc, TDest> mapMethod) //: base(mapMethod)
        {
            Map = mapMethod;
        }

        public Action<TSrc, TDest> Map { get; set; }
    }
}