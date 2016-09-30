using System.Collections.Generic;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests
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

        [Test]
        public void MapList_ListIsTarget_Success()
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

        //[Test]
        //public void MapList_ListIsMappedProperty_Success()
        //{
        //    var config = new HappyConfig(cfg =>
        //    {
        //        cfg.CreateMap<Src, Dest>();
        //        cfg.CreateMap<SrcWrap, DestWrap>();
        //    });
        //    config.AssertConfigurationIsValid();
        //    var mapper = config.CompileMapper();

        //    var srcList = new List<Src>();
        //    srcList.Add(new Src { Value1 = 1 });

        //    SrcWrap srcWrap = new SrcWrap() { P1 = srcList };

        //    DestWrap destWrap = new DestWrap();
        //    mapper.Map<SrcWrap, DestWrap>(srcWrap, destWrap);

        //    Assert.AreEqual(srcWrap.P1[0].Value1, destWrap.P1[0].Value1);
        //}
    }
}