using System.Collections.Generic;
using System.Linq;
using HappyMapper.Tests.PublicAPI;

namespace HappyMapper
{
    public static class Mapper_SrcWrapLevel2_DestWrapLevel2_e32945d9f7bd48e99ada0cca3b06c392
    {
        public static MapCollections_Tests.DestWrapLevel2
            Map
            (
            MapCollections_Tests.SrcWrapLevel2 src,
            MapCollections_Tests.DestWrapLevel2 dest
            )
        {
            if (src.P1 != null)
                dest.P1 = new List<List<MapCollections_Tests.Dest>>();

            dest.P1.Fill(src.P1.Count, () => new List<MapCollections_Tests.Dest>());

            for (var index_395e = 0; index_395e < src.P1.Count; index_395e++)
            {
                var src_6303 = src.P1.ElementAt(index_395e);
                var dest_330d = dest.P1.ElementAt(index_395e);

                //THE PROBLEMs: IF, dest_330d not in dest.P1 collection
                if (src_6303 != null)
                    dest_330d = new List<MapCollections_Tests.Dest>();

                dest_330d.Fill(src_6303.Count, () => new MapCollections_Tests.Dest());

                for (var index_67dc = 0; index_67dc < src_6303.Count; index_67dc++)
                {
                    var src_a755 = src_6303.ElementAt(index_67dc);
                    var dest_a508 = dest_330d.ElementAt(index_67dc);
                    dest_a508.Value1 = src_a755.Value1;

                }

                dest.P1[index_395e] = dest_330d;
            }


            ;
            return dest;
            ;
        }
    }
}