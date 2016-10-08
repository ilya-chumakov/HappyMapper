using System;
using System.Collections.Generic;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Benchmarks.Types;
using HappyMapper.Compilation;
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

        [Test]
        public void Mapper_EmptyDelegateCache_ThrowsExOnMap()
        {
            Assert.Throws(Is.TypeOf<HappyMapperException>()
                .And.Message.ContainsSubstring("Missing mapping"),
                delegate
                {
                    var mapper = new Mapper(new Dictionary<TypePair, CompiledDelegate>());

                    mapper.Map(new Src(), new Dest());
                });
        }
    }
}
