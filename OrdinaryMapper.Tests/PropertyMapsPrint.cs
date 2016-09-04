using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;
using OrdinaryMapperAmcApi.Tests;

namespace OrdinaryMapper.Tests.New
{
    public class PropertyMapsPrint
    {
        public class A { public string MyProperty { get; set; } }
        public class B { public string MyProperty { get; set; } }
        public class C { public string MyProperty { get; set; } }
        public class D { public string MyProperty { get; set; } }
        public class X { public string Foo { get; set; } }

        [Test]
        public void MapperConfiguration_CreateMap()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>().ForMember(d => d.MyProperty, opt => opt.Ignore());
                cfg.CreateMap<C, D>().ForMember(d => d.MyProperty, opt => opt.MapFrom(s => s.MyProperty));
                cfg.CreateMap<X, A>().ForMember(d => d.MyProperty, opt => opt.MapFrom(s => s.Foo));
            });

            PrintPropertyMaps(config);
        }

        private static void PrintPropertyMaps(HappyConfig config)
        {
            var propertyMaps = config.TypeMaps.Values.SelectMany(tm => tm.PropertyMaps).ToList();

            foreach (var propertyMap in propertyMaps)
            {
                Console.WriteLine(propertyMap.ToString());
            }
        }

        //[Test]
        //public void TypeDetails_Ctor_Success()
        //{
        //    var td = new TypeDetails(typeof(A), null);
        //    Assert.IsNotNull(td);
        //}

    }
}