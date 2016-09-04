﻿using System;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Obsolete;
using OrdinaryMapper.Tests.Tools;
using OrdinaryMapperAmcApi.Tests;

namespace OrdinaryMapper.Tests
{
    public class MapperConfiguration_Tests
    {
        [Test]
        public void MapperConfiguration_MapSimpleReferenceTypes_Success()
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
        public void MapperConfiguration_MapNestedReferenceTypes_Success()
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
}
