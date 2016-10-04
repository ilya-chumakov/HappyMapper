using System;
using System.IO;
using HappyMapper.Benchmarks.Types;
using NUnit.Framework;

namespace HappyMapper.Tests.AutoMapper.Extended
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
