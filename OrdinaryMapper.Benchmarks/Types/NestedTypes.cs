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
            Float = DateTime.Now.Millisecond / random.Next();
            DateTime = DateTime.Now;

            Child = new NestedSrcChild();
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
    }

    public class NestedDest
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public float Float { get; set; }
        public DateTime DateTime { get; set; }
        public NestedDestChild Child { get; private set; }
    }

    public class NestedDestChild
    {
        public int MyProperty { get; set; }
    }
}
