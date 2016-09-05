using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests.MemberConfigurationExpressionTests
{
    public class MapFrom
    {
        public class A { public int P1 { get; set; } }
        public class B { public int P2 { get; set; } }

        [Test]
        public void MapFrom_SetMemberWithDifferentName_MemberIsMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.P2, opt => opt.MapFrom(s => s.P1));
            });

            var mapper = config.CompileMapper();

            var a = new A {P1 = 1};
            var b = new B {P2 = 2};

            mapper.Map(a, b);

            Assert.AreEqual(1, b.P2);
        }
    }
}