using NUnit.Framework;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class OrdinaryMapper_Tests
    {
        [Test]
        public void OrdinaryMapper_Map_Success()
        {
            OrdinaryMapper mapper = OrdinaryMapper.Instance;
            mapper.CreateMap<Src, Dest>();

            var src = new Src();
            var dest = new Dest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }
    }
}
