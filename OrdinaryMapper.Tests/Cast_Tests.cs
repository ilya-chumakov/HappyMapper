using System;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests
{
    public class Cast_Tests
    {
        public class SrcByte { public byte Value { get; set; } }
        public class SrcInt { public int Value { get; set; } }
        public class SrcDouble { public double Value { get; set; } }

        public class DestByte { public byte Value { get; set; } }
        public class DestInt { public int Value { get; set; } }
        public class DestDouble { public double Value { get; set; } }
        public class DestStr { public string Value { get; set; } }
        public class DestDt { public DateTime Value { get; set; } }

        [Test]
        public void Int_To_Byte_Success() { Fixture<SrcInt, DestByte>(); }

        [Test]
        public void Int_To_Double_Success() { Fixture<SrcInt, DestDouble>(); }

        [Test]
        public void Byte_To_Int_Success() { Fixture<SrcByte, DestInt>(); }

        [Test]
        public void Double_To_Int_Success() { Fixture<SrcDouble, DestInt>(); }


        public void Fixture<TSrc, TDest>()
            where TSrc : new()
            where TDest : new()
        {
            dynamic src = new TSrc();
            src.Value = 13;

            dynamic dest = Map<TSrc, TDest>(src);

            Assert.AreEqual(src.Value, dest.Value);
        }

        public TDest Map<TSrc, TDest>(TSrc src) where TDest : new()
        {
            var config = new HappyConfig(cfg =>
                cfg.CreateMap<TSrc, TDest>());

            config.AssertConfigurationIsValid();

            var mapper = config.CompileMapper();

            TDest dest = new TDest();
            mapper.Map(src, dest);

            return dest;
        }
    }
}