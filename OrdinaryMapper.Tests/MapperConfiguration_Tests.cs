using System;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class MapperConfiguration_Tests
    {
        [Test]
        public void MapperConfiguration_MapSimpleReferenceTypes_Success()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Src, Dest>();
            });

            Mapper mapper = config.CompileMapper();

            var src = new Src();
            var dest = new Dest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void MapperConfiguration_MapNestedReferenceTypes_Success()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NestedSrc, NestedDest>();
            });

            Mapper mapper = config.CompileMapper();

            var src = new NestedSrc();
            var dest = new NestedDest();

            mapper.Map(src, dest);

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);
            Assert.IsTrue(result.Success);
        }
    }
}
