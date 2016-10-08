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