using System;
using System.Collections.Generic;
using System.Linq;
using OrdinaryMapper;
namespace OrdinaryMapper
{
    public static class Mapper_Src_Dest_df88dc4c4a6d49e199754eb3c3ee7bb8
    {
        public static void Map
 (OrdinaryMapper.Benchmarks.Types.Src src,
  OrdinaryMapper.Benchmarks.Types.Dest dest)
        {
            dest.DateTime = src.DateTime;
            dest.Float = src.Float;
            dest.Number = src.Number;
            dest.Name = src.Name;

        }

    }
}

namespace OrdinaryMapper
{
    public static class Mapper_Src_Dest_63c3433ac1d042bba300c59e40ff6a56
    {
        public static void Map
        (ICollection<OrdinaryMapper.Benchmarks.Types.Src> srcList,
         ICollection<OrdinaryMapper.Benchmarks.Types.Dest> destList)
        {
            if (srcList.Count != destList.Count) throw new NotImplementedException("srcList.Count != destList.Count");
            for (int i = 0; i < srcList.Count; i++)
            {
                var src = srcList.ElementAt(i);
                var dest = destList.ElementAt(i);
                dest.DateTime = src.DateTime;
                dest.Float = src.Float;
                dest.Number = src.Number;
                dest.Name = src.Name;

            }
        }

    }
}
