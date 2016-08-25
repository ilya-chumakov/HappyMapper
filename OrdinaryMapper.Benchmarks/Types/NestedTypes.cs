using System;

namespace OrdinaryMapper.Benchmarks.Types
{
    public class NestedSrc
    {
        public NestedSrc()
        {
            var random = new Random();

            Name = Guid.NewGuid().ToString();
            Number = random.Next();
            Float = DateTime.Now.Millisecond / random.Next(500);
            DateTime = DateTime.Now;

            Child = new NestedSrcChild();
            Child.MyProperty = random.Next(100);
            Child.GrandChild = new NestedSrcGrandChild();
            Child.GrandChild.Foo = (byte) random.Next(byte.MaxValue);
        }

        public string Name { get; private set; }
        public int Number { get; private set; }
        public float Float { get; private set; }
        public DateTime DateTime { get; private set; }
        public NestedSrcChild Child { get; private set; }
    }

    public class NestedSrcChild
    {
        public int MyProperty { get; set; }
        public NestedSrcGrandChild GrandChild { get; set; }
    }

    public class NestedSrcGrandChild
    {
        public byte Foo { get; set; }
    }

    public class NestedDest
    {
        public NestedDest()
        {
            Child = new NestedDestChild();
            Child.GrandChild = new NestedDestGrandChild();
        }

        public string Name { get; set; }
        public int Number { get; set; }
        public float Float { get; set; }
        public DateTime DateTime { get; set; }
        public NestedDestChild Child { get; set; }
    }

    public class NestedDestChild
    {
        public int MyProperty { get; set; }
        public NestedDestGrandChild GrandChild { get; set; }
    }

    public class NestedDestGrandChild
    {
        public byte Foo { get; set; }
    }
}
