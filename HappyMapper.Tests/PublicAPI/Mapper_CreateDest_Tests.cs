using System;
using System.Collections.Generic;
using HappyMapper.Tests.Tools;
using NUnit.Framework;

namespace HappyMapper.Tests.PublicAPI
{
    public class Mapper_CreateDest_Tests
    {
        public class Src
        {
            public int Id { get; set; }

            public SrcChild Child { get; set; }
        }

        public class Dest
        {
            public int Id { get; set; }
            public DestChild Child { get; set; }
        }

        public class SrcChild
        {
            public int P1 { get; set; }
        }

        public class DestChild
        {
            public int P1 { get; set; }
        }

        [Test]
        public void Mapper_MapSimpleTypes_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });
            var mapper = config.CompileMapper();

            var src = new Src { Id = 1 };

            var dest = mapper.Map<Dest>(src);

            var result = ObjectComparer.AreEqual(src, dest);
            result.Errors.ForEach(Console.WriteLine);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Mapper_MapNestedTypes_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });
            var mapper = config.CompileMapper();

            var src = new Src { Id = 1, Child = new SrcChild() { P1 = 2 } };

            var dest = mapper.Map<Dest>(src);

            var result = ObjectComparer.AreEqual(src, dest);
            result.Errors.ForEach(Console.WriteLine);
            Assert.IsTrue(result.Success);
        }
    }

    public class Mapper_CreateDestCollection_Tests
    {
        public class Src
        {
            public int Id { get; set; }
        }

        public class Dest
        {
            public int Id { get; set; }
        }


        [Test]
        public void Map_ListIsTarget_Fail()
        {
            var config = new HappyConfig(cfg => cfg.CreateMap<Src, Dest>());
            config.AssertConfigurationIsValid();
            var mapper = config.CompileMapper();


            var srcList = new List<Src>();
            srcList.Add(new Src { Id = 1 });

            var destList = mapper.MapCollection2<List<Src>, List<Dest>>(srcList);

            Assert.AreNotEqual(srcList[0].Id, destList[0].Id);
        }
    }
}