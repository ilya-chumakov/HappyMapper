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

namespace HappyMapper
{
    public static class Mapper_Src_Dest_6cc8f6f5f6b943b09e9460f56e2cb59e
    {
        public static MapCollections_Tests.Dest
            Map
            (
            MapCollections_Tests.Src src,
            MapCollections_Tests.Dest dest
            )
        {
            dest.Value1 = src.Value1;
            ;
            return dest;
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_SrcWrapLevel2_DestWrapLevel2_2dd4e6e7db964204871fc023fbb0ae7d
    {
        public static ICollection<MapCollections_Tests.DestWrapLevel2>
            MapCollection
            (
            ICollection<MapCollections_Tests.SrcWrapLevel2> srcList,
            ICollection<MapCollections_Tests.DestWrapLevel2> destList
            )
        {
            for (var index_3965 = 0; index_3965 < srcList.Count; index_3965++)
            {
                var src = srcList.ElementAt(index_3965);
                var dest = destList.ElementAt(index_3965);
                if (src.P1 != null)
                    dest.P1 = new List<List<MapCollections_Tests.Dest>>();
                dest.P1.Fill(src.P1.Count, () => new List<MapCollections_Tests.Dest>());
                for (var index_395e = 0; index_395e < src.P1.Count; index_395e++)
                {
                    var src_6303 = src.P1.ElementAt(index_395e);
                    var dest_330d = dest.P1.ElementAt(index_395e);
                    if (src_6303 != null)
                        dest_330d = new List<MapCollections_Tests.Dest>();
                    dest_330d.Fill(src_6303.Count, () => new MapCollections_Tests.Dest());
                    for (var index_67dc = 0; index_67dc < src_6303.Count; index_67dc++)
                    {
                        var src_a755 = src_6303.ElementAt(index_67dc);
                        var dest_a508 = dest_330d.ElementAt(index_67dc);
                        dest_a508.Value1 = src_a755.Value1;

                    }


                }


            }
            ;
            return destList;
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_Src_Dest_d148ae937f4740edb903a376beb071f6
    {
        public static ICollection<MapCollections_Tests.Dest>
            MapCollection
            (
            ICollection<MapCollections_Tests.Src> srcList,
            ICollection<MapCollections_Tests.Dest> destList
            )
        {
            for (var index_e003 = 0; index_e003 < srcList.Count; index_e003++)
            {
                var src = srcList.ElementAt(index_e003);
                var dest = destList.ElementAt(index_e003);
                dest.Value1 = src.Value1;

            }
            ;
            return destList;
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_SrcWrapLevel2_DestWrapLevel2_843454848fce4e9a930e310def3a6e46
    {
        public static void
            MapCollection
            (
            object srcList,
            object destList
            )
        {
            var src = srcList as ICollection<MapCollections_Tests.SrcWrapLevel2>;
            var dest = destList as ICollection<MapCollections_Tests.DestWrapLevel2>;
            dest.Fill(src.Count, () => new MapCollections_Tests.DestWrapLevel2());
            Mapper_SrcWrapLevel2_DestWrapLevel2_2dd4e6e7db964204871fc023fbb0ae7d.MapCollection(
                src,
                dest
                )
                ;
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_Src_Dest_32c3805a6659458b8270668cf4c13b33
    {
        public static void
            MapCollection
            (
            object srcList,
            object destList
            )
        {
            var src = srcList as ICollection<MapCollections_Tests.Src>;
            var dest = destList as ICollection<MapCollections_Tests.Dest>;
            dest.Fill(src.Count, () => new MapCollections_Tests.Dest());
            Mapper_Src_Dest_d148ae937f4740edb903a376beb071f6.MapCollection(
                src,
                dest
                )
                ;
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_SrcWrapLevel2_DestWrapLevel2_4450ef461e3d4888ac420a9ba9f7404e
    {
        public static MapCollections_Tests.DestWrapLevel2
            Map
            (
            object src
            )
        {
            ;
            return Mapper_SrcWrapLevel2_DestWrapLevel2_e32945d9f7bd48e99ada0cca3b06c392.Map(
                src as MapCollections_Tests.SrcWrapLevel2,
                new MapCollections_Tests.DestWrapLevel2()
                );
            ;
        }
    }
}

namespace HappyMapper
{
    public static class Mapper_Src_Dest_0bf68c30f57142ec900d503d2d69a1f2
    {
        public static MapCollections_Tests.Dest
            Map
            (
            object src
            )
        {
            ;
            return Mapper_Src_Dest_6cc8f6f5f6b943b09e9460f56e2cb59e.Map(
                src as MapCollections_Tests.Src,
                new MapCollections_Tests.Dest()
                );
            ;
        }
    }
}