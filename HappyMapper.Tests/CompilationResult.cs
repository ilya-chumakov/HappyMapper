using System.Collections.Generic;
using System.Linq;
using HappyMapper.Tests.PublicAPI;

namespace HappyMapper
{
    public static class Mapper_SrcWrapLevel2_DestWrapLevel2_8bae
    {
        public static MapCollections_Tests.DestWrapLevel2
            Map
            (
            MapCollections_Tests.SrcWrapLevel2 src,
            MapCollections_Tests.DestWrapLevel2 dest
            )
        {
            if (src.P1 == null)
            {
                dest.P1 = null;
            }
            else
            {
                if (dest.P1 == null || src.P1.Count != dest.P1.Count)
                {
                    dest.P1 = new List<List<MapCollections_Tests.Dest>>();
                    dest.P1.Add(src.P1.Count, () => new List<MapCollections_Tests.Dest>());
                }
                for (var index_26ef = 0; index_26ef < src.P1.Count; index_26ef++)
                {
                    var src_6830 = src.P1.ElementAt(index_26ef);
                    var dest_12e0 = dest.P1.ElementAt(index_26ef);

                    if (dest_12e0 == null)
                    {
                        dest_12e0 = new List<MapCollections_Tests.Dest>();
                    }
                    else
                    {
                        dest_12e0.Clear();
                    }

                    dest_12e0.Add(src_6830.Count, () => new MapCollections_Tests.Dest());

                    for (var index_8e02 = 0; index_8e02 < src_6830.Count; index_8e02++)
                    {
                        var src_0baf = src_6830.ElementAt(index_8e02);
                        var dest_ddf0 = dest_12e0.ElementAt(index_8e02);
                        dest_ddf0.Value1 = src_0baf.Value1;

                    }


                }


            }
            ;
            return dest;
            ;
        }
    }
}