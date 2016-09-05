using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests.MemberConfigurationExpressionTests
{
    public class Ignore
    {
        public class A { public int P1 { get; set; } }
        public class B { public int P1 { get; set; } }
        public class C { public int P1 { get; set; } public int P2 { get; set; } }

        [Test]
        public void Ignore_IgnoreMappableDestinationMember_MemberIsNotMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Ignore());
            });

            var mapper = config.CompileMapper();

            var a = new A {P1 = 1};
            var b = new B {P1 = 2};

            mapper.Map(a, b);

            Assert.AreEqual(2, b.P1);
        }

        [Test]
        public void Ignore_IgnoreNotMappedDestinationMember_ConfigurationIsValid()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, C>().ForMember(d => d.P2, opt => opt.Ignore());
            });

            Assert.DoesNotThrow(() => config.AssertConfigurationIsValid());
        }

    }
}