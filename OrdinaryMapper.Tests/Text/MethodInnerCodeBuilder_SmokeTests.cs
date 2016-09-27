using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.ConfigurationAPI;
using NUnit.Framework;
using OrdinaryMapper.AmcApi;
using OrdinaryMapperAmcApi.Tests;

namespace OrdinaryMapper.Tests.Text
{
    public class MethodInnerCodeBuilder_SmokeTests
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
            var config = new HappyConfig(cfg =>
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
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<A1, B1>().ForMember(d => d.Id, opt => opt.Ignore());
                cfg.CreateMap<A2, B2>();
            });

            var typeMaps = config.TypeMaps;
            var mtb = new MethodInnerCodeBuilder(typeMaps, config.Configuration);

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                string text = mtb.CreateMethodInnerCode(map).Code;

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_FullExplicitMap()
        {
            var config = new HappyConfig(cfg =>
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

                var mtb = new MethodInnerCodeBuilder(typeMaps, config.Configuration);
                string text = mtb.CreateMethodInnerCode(map).Code;

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_RootLevelOnlyMapped()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>();
            });

            var typeMaps = config.TypeMaps;

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mtb = new MethodInnerCodeBuilder(typeMaps, config.Configuration);
                string text = mtb.CreateMethodInnerCode(map).Code;

                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void NestedType_Test()
        {
            var x = new OrdinaryMapper.Tests.Text.MethodInnerCodeBuilder_SmokeTests.B1();
            Assert.IsNotNull(x);
        }
    }
}