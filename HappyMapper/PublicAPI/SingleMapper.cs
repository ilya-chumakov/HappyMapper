using System;

namespace HappyMapper
{
    public class SingleMapper<TSrc, TDest>
    {
        public SingleMapper(Action<TSrc, TDest> mapMethod)
        {
            Map = mapMethod;
        }

        public Action<TSrc, TDest> Map { get; set; }
    }
}