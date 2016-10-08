using System;
using System.Collections.Generic;
using System.Linq;
using HappyMapper;
namespace HappyMapper
{
    public static class Mapper_Src_Dest_e254d33d650b4f6fa639fe92214fec9f
    {
        public static HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest
        Map
         (
         HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Src src,
 HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest dest
         )
        {
            if (src == null) dest = null;
            else
            {
                dest.Child = new HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.DestChild();
                dest.Child.P1 = src.Child.P1;

            }

            dest.Id = src.Id;

            return dest;
        }

    }
}
namespace HappyMapper
{
    public static class Mapper_Src_Dest_1dafd6cc8e394cb8bb111806ff24ed2b
    {
        public static ICollection<HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest>
        MapCollection
         (
         ICollection<HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Src> srcList,
 ICollection<HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest> destList
         )
        {
            for (int index_260f = 0; index_260f < srcList.Count; index_260f++)
            {
                var src = srcList.ElementAt(index_260f);
                var dest = destList.ElementAt(index_260f);
                if (src == null) dest = null;
                else
                {
                    dest.Child = new HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.DestChild();
                    dest.Child.P1 = src.Child.P1;

                }

                dest.Id = src.Id;

            }

            return destList;
        }

    }
}
namespace HappyMapper
{
    public static class Mapper_Src_Dest_5640ae92714a4ac08a54f8716a096049
    {
        public static HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest
        Map
         (
         Object src
         )
        {

            return HappyMapper.Mapper_Src_Dest_e254d33d650b4f6fa639fe92214fec9f.Map(
    src as HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Src,
    new HappyMapper.Tests.PublicAPI.Mapper_CreateDest_Tests.Dest()
    );
        }

    }
}