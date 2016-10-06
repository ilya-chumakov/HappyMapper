using System;
using HappyMapper.Benchmarks.Types;
using HappyMapper.Tests.Tools;
using NUnit.Framework;

namespace HappyMapper.Tests.PublicAPI
{
    public class Mapper_Common_Tests
    {
        [Test]
        public void Mapper_MapSimpleReferenceTypes_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });
            var mapper = config.CompileMapper();

            var src = new Src();
            var dest = new Dest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Mapper_MapNestedReferenceTypes_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<NestedSrc, NestedDest>();
            });
            var mapper = config.CompileMapper();

            var src = new NestedSrc();
            var dest = new NestedDest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);
            Assert.IsTrue(result.Success);
        }
    }

    public class Mapper_InitDestPropertyWithNewObject_Tests
    {
        public class Src { public SrcChild Child { get; set; } }
        public class Dest { public DestChild Child { get; set; } }
        public class SrcChild { public int P1 { get; set; } }
        public class DestChild { public int P1 { get; set; } }

        [Test]
        public void Mapper_MapNullDestChild_CreatesDestChild()
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
    }
}
