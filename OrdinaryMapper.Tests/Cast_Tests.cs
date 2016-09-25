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
        public class SrcStr { public string Value { get; set; } }

        public class DestByte { public byte Value { get; set; } }
        public class DestInt { public int Value { get; set; } }
        public class DestDouble { public double Value { get; set; } }
        public class DestDecimal { public decimal Value { get; set; } }
        public class DestStr { public string Value { get; set; } }
        public class DestDt { public DateTime Value { get; set; } }

        [Test]
        public void Int_To_Byte_Success() { Fixture_MapFromValueType<SrcInt, DestByte>(); }

        [Test]
        public void Int_To_Double_Success() { Fixture_MapFromValueType<SrcInt, DestDouble>(); }

        [Test]
        public void Int_To_String_Success() { Fixture_MapFromValueType<SrcInt, DestStr>(s => s.Value.ToString()); }

        [Test]
        public void Byte_To_Int_Success() { Fixture_MapFromValueType<SrcByte, DestInt>(); }

        [Test]
        public void Double_To_Int_Success() { Fixture_MapFromValueType<SrcDouble, DestInt>(); }

        [Test]
        public void String_To_Int_Success() { Fixture_MapFromString<SrcStr, DestInt>(); }

        [Test]
        public void String_To_Double_Success() { Fixture_MapFromString<SrcStr, DestDouble>(); }

        [Test]
        public void String_To_Decimal_Success() { Fixture_MapFromString<SrcStr, DestDecimal>(); }

        public void Fixture_MapFromValueType<TSrc, TDest>(Func<TSrc, object> srcValueGetter = null)
            where TSrc : new()
            where TDest : new()
        {
            dynamic src = new TSrc();
            src.Value = 13;

            dynamic dest = Map<TSrc, TDest>(src);

            Assert.AreEqual(srcValueGetter == null ? src.Value : srcValueGetter(src), dest.Value);
        }

        public void Fixture_MapFromString<TSrc, TDest>()
            where TSrc : new()
            where TDest : new()
        {
            dynamic src = new TSrc();
            src.Value = 13.ToString();

            dynamic dest = Map<TSrc, TDest>(src);

            Assert.AreEqual(src.Value, dest.Value.ToString());
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