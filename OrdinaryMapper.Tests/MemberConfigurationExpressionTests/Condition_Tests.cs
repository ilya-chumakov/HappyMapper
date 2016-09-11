using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapper.Tests.MemberConfigurationExpressionTests
{
    public class Condition_Tests
    {
        public class A { public int P1 { get; set; } }
        public class B { public int P1 { get; set; } }
        public class X { public int P2 { get; set; } }
        public class C { public A A { get; set; } }
        public class D { public B B { get; set; } }

        [Test]
        public void Condition_NotSatisfied_MemberIsNotMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s, d) => s.P1 + d.P1 < 0));
                //cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s, d) => DateTime.Now.Ticks < 0 ));
                //cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s) => s.P1 > 0 ));
            });

            var mapper = config.CompileMapper();

            int newValue = 1;
            int originValue = 5;
            var a = new A { P1 = newValue };
            var b = new B { P1 = originValue };

            mapper.Map(a, b);

            Assert.AreEqual(originValue, b.P1);
        }

        [Test]
        public void Condition_Satisfied_MemberIsMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.P1, opt => opt.Condition((s, d) => s.P1 + d.P1 < 0));
            });

            var mapper = config.CompileMapper();


            int newValue = -10;
            int originValue = 5;
            var a = new A { P1 = newValue };
            var b = new B { P1 = originValue };

            mapper.Map(a, b);

            Assert.AreEqual(newValue, b.P1);
        }

        [Test]
        public void Condition_SatisfiedWithMapFrom_MemberIsMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, X>().ForMember(d => d.P2, opt =>
                {
                    opt.MapFrom(s => s.P1);
                    opt.Condition((s, d) => s.P1 + d.P2 < 0);
                });
            });

            var mapper = config.CompileMapper();

            int newValue = -10;
            int originValue = 5;
            var a = new A { P1 = newValue };
            var x = new X { P2 = originValue };

            mapper.Map(a, x);

            Assert.AreEqual(newValue, x.P2);
        }

        [Test]
        public void Condition_NotSatisfiedWithMapFrom_MemberNotIsMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, X>().ForMember(d => d.P2, opt =>
                {
                    opt.MapFrom(s => s.P1);
                    opt.Condition((s, d) => s.P1 + d.P2 < 0);
                });
            });

            var mapper = config.CompileMapper();

            int newValue = 1;
            int originValue = 5;
            var a = new A { P1 = newValue };
            var x = new X { P2 = originValue };

            mapper.Map(a, x);

            Assert.AreEqual(originValue, x.P2);
        }

        [Test]
        public void Condition_NotSatisfiedOnASubClass_SubclassIsNotMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(dest => dest.P1, opt => opt.Condition((ps, pd) => ps.P1 + pd.P1 < 0));
                cfg.CreateMap<C, D>();
            });

            var mapper = config.CompileMapper();

            int newValue = 1;
            int originValue = 5;
            var a = new A { P1 = newValue };
            var b = new B { P1 = originValue };
            var c = new C { A = a };
            var d = new D { B = b };

            mapper.Map(c, d);

            Assert.AreEqual(originValue, b.P1);
        }
    }
}