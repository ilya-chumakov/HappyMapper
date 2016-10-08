using System;

namespace HappyMapper
{
    public class SingleMapper<TSrc, TDest>
    {
        public SingleMapper(Func<TSrc, TDest, TDest> mapMethod)
        {
            Map = mapMethod;
        }

        public Func<TSrc, TDest, TDest> Map { get; set; }
    }
}