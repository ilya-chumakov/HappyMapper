using System;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests.MemberConfigurationExpressionTests
{
    public class Condition_Tests
    {
        public class A { public int P1 { get; set; } }
        public class B { public int P1 { get; set; } }

        [Test]
        public void Condition_NotSatisfied_MemberIsNotMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s, d) => s.P1 + d.P1 > 0 ));
                cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s, d) => DateTime.Now.Ticks < 0 ));
                //cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s) => s.P1 > 0 ));
            });

            var mapper = config.CompileMapper();

            var a = new A { P1 = -1 };
            var b = new B { P1 = 2 };

            mapper.Map(a, b);

            Assert.AreEqual(2, b.P1);
        }
    }
}