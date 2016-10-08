using System;
using System.Collections.Generic;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Text;
using NUnit.Framework;

namespace HappyMapper.Tests.Text
{
    public class TextBuilder_SmokeTests
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
            var mtb = new TextBuilder(typeMaps, config.Configuration);

            var files1 = mtb.CreateCodeFiles();
            var files = files1;

            Print(files);
        }

        [Test]
        public void OneArgTextBuilder_CreateCodeFiles_Success()
        {
            var config = new HappyConfig(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<A1, B1>().ForMember(d => d.Id, opt => opt.Ignore());
                cfg.CreateMap<A2, B2>();
            });

            var typeMaps = config.TypeMaps;
            var mtb = new TextBuilder(typeMaps, config.Configuration);

            var files = mtb.CreateCodeFiles();

            var a1tb = new OneArgTextBuilder(typeMaps, config.Configuration);

            var a1files = a1tb.CreateCodeFiles(files);

            Print(a1files);
        }

        private static void Print(Dictionary<TypePair, CodeFile> files)
        {
            foreach (var file in files.Values)
            {
                Console.WriteLine(file.Code);
                Console.WriteLine("--------------------------------------");
                Console.WriteLine();
            }
        }
    }
}