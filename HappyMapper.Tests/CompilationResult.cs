using System.Collections.Generic;
using System.Linq;
using AutoMapper.Extended.Net4.SharedTools;
using HappyMapper.Tests.PublicAPI;

namespace HappyMapper.Tests
{
    public static class Mapper_Src_Dest_c493
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

    public static class Mapper_Src_Dest_3fd5
    {
        public static ICollection<MapCollections_Tests.Dest>
            MapCollection
            (
            ICollection<MapCollections_Tests.Src> srcList,
            ICollection<MapCollections_Tests.Dest> destList
            )
        {
            for (var index_ac65 = 0; index_ac65 < srcList.Count; index_ac65++)
            {
                var src = srcList.ElementAt(index_ac65);
                var dest = destList.ElementAt(index_ac65);
                dest.Value1 = src.Value1;

            }
            ;
            return destList;
            ;
        }
    }

    public static class Mapper_Src_Dest_a883
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
            dest.Add(src.Count, () => new MapCollections_Tests.Dest());
            Mapper_Src_Dest_3fd5.MapCollection(
                src,
                dest
                )
                ;
            ;
        }
    }

    public static class Mapper_Src_Dest_c5a5
    {
        public static MapCollections_Tests.Dest
            Map
            (
            object src
            )
        {
            ;
            return Mapper_Src_Dest_c493.Map(
                src as MapCollections_Tests.Src,
                new MapCollections_Tests.Dest()
                );
            ;
        }
    }
}