using NUnit.Framework;

namespace OrdinaryMapper.Tests.New
{
    public class Tests
    {
        public class A { public string MyProperty { get; set; } }
        public class B { public string MyProperty { get; set; } }
        public class C { public string MyProperty { get; set; } }
        public class D { public string MyProperty { get; set; } }

        [Test]
        public void MapperConfiguration_CreateMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.MyProperty, opt => opt.Ignore());
                cfg.CreateMap<C, D>();
            });
        }

        [Test]
        public void TypeDetails_Ctor_Success()
        {
            var td = new TypeDetails(typeof(A));
            Assert.IsNotNull(td);
        }
    }
}