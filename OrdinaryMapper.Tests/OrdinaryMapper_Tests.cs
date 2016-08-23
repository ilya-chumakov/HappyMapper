using NUnit.Framework;
using OrdinaryMapper.Benchmarks;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class OrdinaryMapper_Tests
    {
        [Test]
        public void OrdinaryMapper_MapSimpleReferenceTypes_Success()
        {
            Mapper mapper = Mapper.Instance;
            mapper.CreateMap<Src, Dest>();

            var src = new Src();
            var dest = new Dest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void OrdinaryMapper_MapNestedReferenceTypes_Success()
        {
            Mapper mapper = Mapper.Instance;
            mapper.CreateMap<NestedSrc, NestedDest>();

            var src = new NestedSrc();
            var dest = new NestedDest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }
    }
}
