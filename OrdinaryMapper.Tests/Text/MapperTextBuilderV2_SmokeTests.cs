using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class MapperTextBuilderV2_SmokeTests
    {
        public class A
        {
            public string Name { get; set; }
            public A1 Child { get; set; }
        }
        public class A1 { public int Id { get; set; } public A2 SubChild { get; set; } }
        public class A2 { public DateTime Date { get; set; } }

        public class B
        {
            public string Name { get; set; }
            public B1 Child { get; set; }
        }
        public class B1 { public int Id { get; set; } public B2 SubChild { get; set; } }
        public class B2 { public DateTime Date { get; set; } }

        [Test]
        public void CreateCodeFiles_Success()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<A1, B1>().ForMember(d => d.Id, opt => opt.Ignore());
                cfg.CreateMap<A2, B2>();
            });

            var typeMaps = config.TypeMaps;
            var mtb = new MapperTextBuilderV2(typeMaps, config.Configuration);

            var files = mtb.CreateCodeFiles();

            foreach (var file in files.Values)
            {
                Console.WriteLine(file.Code);
                Console.WriteLine("--------------------------------------");
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_IgnoreAtSubLevel()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<A1, B1>().ForMember(d => d.Id, opt => opt.Ignore());
                cfg.CreateMap<A2, B2>();
            });

            var typeMaps = config.TypeMaps;
            var mtb = new MapperTextBuilderV2(typeMaps, config.Configuration);

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                string text = mtb.CreateMethodInnerCode(map);

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_FullExplicitMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<A1, B1>();
                cfg.CreateMap<A2, B2>();
            });

            var typeMaps = config.TypeMaps;

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mtb = new MapperTextBuilderV2(typeMaps, config.Configuration);
                string text = mtb.CreateMethodInnerCode(map);

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_RootLevelOnlyMapped()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>();
            });

            var typeMaps = config.TypeMaps;

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mtb = new MapperTextBuilderV2(typeMaps, config.Configuration);
                string text = mtb.CreateMethodInnerCode(map);

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void NestedType_Test()
        {
            var x = new OrdinaryMapper.Tests.Text.MapperTextBuilderV2_SmokeTests.B1();
            Assert.IsNotNull(x);
        }
    }
}