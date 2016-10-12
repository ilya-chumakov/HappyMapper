using System.Collections.Generic;
using NUnit.Framework;

namespace HappyMapper.Tests.PublicAPI
{
    public class MapCollections_Tests
    {
        public class Src
        {
            public int Value1 { get; set; }
        }

        public class Dest
        {
            public int Value1 { get; set; }
        }

        public class SrcWrap
        {
            public List<Src> P1 { get; set; }
        }

        public class DestWrap
        {
            public List<Dest> P1 { get; set; }
        }

        public class SrcWrapLevel2
        {
            public List<List<Src>> P1 { get; set; }
        }

        public class DestWrapLevel2
        {
            public List<List<Dest>> P1 { get; set; }
        }

        [Test]
        public void Map_ListIsTarget_Success()
        {
            var config = new HappyConfig(cfg => cfg.CreateMap<Src, Dest>());
            config.AssertConfigurationIsValid();
            var mapper = config.CompileMapper();
            

            var srcList = new List<Src>();
            srcList.Add(new Src { Value1 = 1 });

            List<Dest> destList = new List<Dest>();
            destList.Add(new Dest());

            mapper.MapCollection(srcList, destList);

            Assert.AreEqual(srcList[0].Value1, destList[0].Value1);
        }

        [Test]
        public void Map_ListIsMappedProperty_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
                cfg.CreateMap<SrcWrap, DestWrap>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CompileMapper();

            var srcList = new List<Src>();
            srcList.Add(new Src { Value1 = 1 });

            List<Dest> destList = new List<Dest>();
            destList.Add(new Dest());

            SrcWrap srcWrap = new SrcWrap { P1 = srcList };
            DestWrap destWrap = new DestWrap { P1 = destList };

            mapper.Map(srcWrap, destWrap);

            Assert.AreEqual(srcWrap.P1[0].Value1, destWrap.P1[0].Value1);
        }

        [Test]
        public void Map_ListOfLists_NotMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
                cfg.CreateMap<SrcWrapLevel2, DestWrapLevel2>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CompileMapper();

            var srcList = new List<Src>();
            var srcListL2 = new List<List<Src>>();
            srcList.Add(new Src { Value1 = 1 });
            srcListL2.Add(srcList);


            var destList = new List<Dest>();
            var destListL2 = new List<List<Dest>>();
            destList.Add(new Dest());
            destListL2.Add(destList);

            var srcWrap = new SrcWrapLevel2 { P1 = srcListL2 };
            var destWrap = new DestWrapLevel2 { P1 = destListL2 };

            //mapper.Map(srcWrap, destWrap);
            Mapper_SrcWrapLevel2_DestWrapLevel2_e32945d9f7bd48e99ada0cca3b06c392.Map(srcWrap, destWrap);

            //It's easy to make things work by modifying AssignCollections method: 
            //dig onto generic arguments in cycle until you met non-collection type at both src and dest ends.
            //However, I think the destination creation module should be implemented first.
            Assert.AreNotEqual(srcWrap.P1[0][0].Value1, destWrap.P1[0][0].Value1);
        }
    }
}