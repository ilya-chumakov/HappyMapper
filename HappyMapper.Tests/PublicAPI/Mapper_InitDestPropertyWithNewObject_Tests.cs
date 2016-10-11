using System.Collections.Generic;
using NUnit.Framework;

namespace HappyMapper.Tests.PublicAPI
{
    public class Mapper_InitDestPropertyWithNewObject_Tests
    {
        public class Src
        {
            public SrcChild Child { get; set; }
        }

        public class Dest
        {
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

        public class SrcWrap
        {
            public List<SrcChild> List { get; set; }
        }

        public class DestWrap
        {
            public List<DestChild> List { get; set; }
        }

        [Test]
        public void Mapper_MapNullDestChild_CreatesObject()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });
            var mapper = config.CompileMapper();

            var src = new Src { Child = new SrcChild() };
            var dest = new Dest();

            Assert.IsNull(dest.Child);

            mapper.Map(src, dest);

            Assert.IsNotNull(dest.Child);
        }

        [Test]
        public void Mapper_MapNullDestList_CreatesList()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<SrcWrap, DestWrap>();
                cfg.CreateMap<SrcChild, DestChild>();
            });
            var mapper = config.CompileMapper();

            var src = new SrcWrap { List = new List<SrcChild> {new SrcChild {P1 = 1} } };
            var dest = new DestWrap();

            Assert.IsNull(dest.List);

            mapper.Map(src, dest);

            Assert.IsTrue(dest.List.Count > 0);
        }
    }
}