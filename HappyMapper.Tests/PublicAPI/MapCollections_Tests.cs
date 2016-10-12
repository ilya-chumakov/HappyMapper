using System;
using System.Collections.Generic;
using HappyMapper.Tests.Tools;
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
            srcList.Add(new Src { Value1 = Gen.Int() });
            srcList.Add(new Src { Value1 = Gen.Int() });
            srcList.Add(new Src { Value1 = Gen.Int() });

            List<Dest> destList = new List<Dest>();
            destList.Add(new Dest());
            destList.Add(new Dest());
            destList.Add(new Dest());

            mapper.Map(srcList, destList);

            ObjectComparer.AreEqualCollections(srcList, destList);
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
        public void Map_ListOfLists_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
                cfg.CreateMap<SrcWrapLevel2, DestWrapLevel2>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CompileMapper();

            var srcListA = new List<Src>();
            
            srcListA.Add(new Src { Value1 = Gen.Int() });
            srcListA.Add(new Src { Value1 = Gen.Int() });
            srcListA.Add(new Src { Value1 = Gen.Int() });

            var srcListB = new List<Src>();

            srcListB.Add(new Src { Value1 = Gen.Int() });
            srcListB.Add(new Src { Value1 = Gen.Int() });

            var srcListL2 = new List<List<Src>>();
            srcListL2.Add(srcListA);
            srcListL2.Add(srcListB);


            var destList = new List<Dest>();
            var destListL2 = new List<List<Dest>>();
            destList.Add(new Dest());
            destListL2.Add(destList);

            var srcWrap = new SrcWrapLevel2 { P1 = srcListL2 };
            var destWrap = new DestWrapLevel2 { P1 = destListL2 };

            mapper.Map(srcWrap, destWrap);
            //Mapper_SrcWrapLevel2_DestWrapLevel2_8bae.Map(srcWrap, destWrap);

            for (int i = 0; i < srcWrap.P1.Count; i++)
            {
                for (int j = 0; j < srcWrap.P1[i].Count; j++)
                {
                    var src = srcWrap.P1[i][j];
                    var dest= destWrap.P1[i][j];

                    var result = ObjectComparer.AreEqual(src, dest);

                    Assert.IsTrue(result.Success);
                }
            }
        }
    }
}