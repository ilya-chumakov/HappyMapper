using NUnit.Framework;
using OrdinaryMapper.AmcApi;

namespace OrdinaryMapperAmcApi.Tests
{
    public class HappyConfig_Tests
    {
        [Test]
        public void MyMethod()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(x => x.Name, opt => opt.Ignore());
                cfg.CreateMap<NestedA, NestedB>();
            });
        }

        public class A
        {
            public string Name { get; set; }
            public NestedA Child { get; set; }
        }

        public class NestedA
        {
            public int Id { get; set; }
        }

        public class B
        {
            public string Name { get; set; }
            public NestedB Child { get; set; }
        }

        public class NestedB
        {
            public int Id { get; set; }
        }
    }
}