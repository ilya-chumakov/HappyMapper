using System;
using System.IO;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;

namespace OrdinaryMapper.Tests.AutoMapper.Extended
{
    public class UnsupportedTypeBlocker_Tests
    {
        [Test]
        public void CreateMap_AbstractType_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var config = new HappyConfig(cfg =>
                {
                    cfg.CreateMap<Src, Stream>();
                });
            });
        }

        [Test]
        public void CreateMap_Interface_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var config = new HappyConfig(cfg =>
                {
                    cfg.CreateMap<ICloneable, Dest>();
                });
            });
        }

        [Test]
        public void CreateMap_Primitive_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var config = new HappyConfig(cfg =>
                {
                    cfg.CreateMap<byte, Dest>();
                });
            });
        }
    }
}
