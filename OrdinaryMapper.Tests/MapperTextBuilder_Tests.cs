using System;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks.Types;

namespace OrdinaryMapper.Tests
{
    public class MapperTextBuilder_Tests
    {
        [Test]
        public void CreateText_SimpleTypes_ProducesText()
        {
            MapContext context = new MapContext(typeof(Src), typeof(Dest));
            string text = MapperTextBuilder.CreateText(context);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);
        }

        [Test]
        public void CreateText_NestedTypes_ProducesText()
        {
            MapContext context = new MapContext(typeof(NestedSrc), typeof(NestedDest));
            string text = MapperTextBuilder.CreateText(context);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);
        }
    }
}