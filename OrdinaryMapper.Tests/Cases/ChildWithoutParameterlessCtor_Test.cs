using System;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.Cases.ChildWithoutParameterlessCtor
{
    public class Src
    {
        public Src()
        {
            Child = new SrcChild();
            Child.MyProperty = 13;
        }
        public SrcChild Child { get; private set; }
    }

    public class SrcChild
    {
        public int MyProperty { get; set; }
    }

    public class Dest
    {
        public DestChild Child { get; set; }
    }

    public class DestChild
    {
        public DestChild(int val)
        {
            MyProperty = val;
        }
        public int MyProperty { get; set; }
    }

    public class ChildWithoutParameterlessCtor_Test
    {
        [Test]
        public void Map_NoDestChildCtor_ThrowsEx()
        {
            Assert.Throws(Is.TypeOf<OrdinaryMapperException>()
                         .And.Message.ContainsSubstring("parameterless ctor"),
                         //.And.Property("MyParam").EqualTo(42),
                delegate
                {
                    Mapper mapper = Mapper.Instance;
                    mapper.CreateMap<Src, Dest>();

                    var src = new Src();
                    var dest = new Dest();

                    mapper.Map(src, dest);
                });
        }

        [Test]
        public void GetText()
        {
            MapContext context = new MapContext(typeof(Src), typeof(Dest));
            string text = MapperTextBuilder.CreateText(context);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);
        }
    }


}