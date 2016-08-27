using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.New
{
    public class MapperTextBuilderV2_Tests
    {
        public class A
        {
            public string Name { get; set; }
            public NestedA Child { get; set; }
        }

        public class NestedA { public int Id { get; set; } }

        public class B
        {
            public string Name { get; set; }
            public NestedB Child { get; set; }
        }

        public class NestedB { public int Id { get; set; } }

        [Test]
        public void CreateMapper_Nested()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<A, B>();
                cfg.CreateMap<NestedA, NestedB>();
            });

            var typeMaps = config.TypeMaps;

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                string text = MapperTextBuilderV2.CreateText(map, typeMaps);
                Console.WriteLine(typePair.ToString());
                Console.WriteLine(text);
                Console.WriteLine();
            }
        }

        [Test]
        public void CreateText_Simple()
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

                string text = MapperTextBuilderV2.CreateText(map, typeMaps);
                Console.WriteLine(text);
            }
        }


        public void CreateMap<TSrc, TDest>(Dictionary<TypePair, TypeMap> typeMaps)
        {
            var typePair = new TypePair(typeof(TSrc), typeof(TDest));

            TypeMap map;
            typeMaps.TryGetValue(typePair, out map);

            if (map == null) typeMaps.Add(typePair, new TypeMap(typePair, typeof(Action<TSrc, TDest>)));
        }
    }
}