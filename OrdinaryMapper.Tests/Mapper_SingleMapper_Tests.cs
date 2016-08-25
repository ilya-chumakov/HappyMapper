using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class Mapper_SingleMapper_Tests
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
        public void GetSingleMapper_MapExists_ReturnsMapper()
        {
            var singleMapper = CreateSingleMapper();

            Assert.IsNotNull(singleMapper);
        }

        private static SingleMapper<Src, Dest> CreateSingleMapper()
        {
            Mapper mapper = new Mapper();
            mapper.CreateMap<Src, Dest>();
            mapper.Compile();
            var singleMapper = mapper.GetSingleMapper<Src, Dest>();
            return singleMapper;
        }
    }
}