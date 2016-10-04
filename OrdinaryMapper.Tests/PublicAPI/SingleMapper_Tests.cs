using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests.PublicAPI
{
    public class SingleMapper_Tests
    {
        [Test]
        public void GetSingleMapper_Map_Success()
        {
            var singleMapper = CreateSingleMapper();

            var src = new Src();
            var dest = new Dest();

            singleMapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void GetSingleMapper_NoMap_ThrowsEx()
        {
            Assert.Throws(Is.TypeOf<OrdinaryMapperException>()
                , () => 
                {
                    var config = new HappyConfig(cfg =>
                    {
                        cfg.CreateMap<NestedSrc, NestedDest>();
                    });
                    var mapper = config.CompileMapper();
                    var singleMapper = mapper.GetSingleMapper<Src, Dest>();
                });
        }

        [Test]
        public void GetSingleMapper_MapExists_ReturnsMapper()
        {
            var singleMapper = CreateSingleMapper();

            Assert.IsNotNull(singleMapper);
        }

        private static SingleMapper<Src, Dest> CreateSingleMapper()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });
            var mapper = config.CompileMapper();

            var singleMapper = mapper.GetSingleMapper<Src, Dest>();
            return singleMapper;
        }
    }
}