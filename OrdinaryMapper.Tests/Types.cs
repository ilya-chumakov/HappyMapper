using System;

namespace OrdinaryMapper.Tests
{
    public class Src
    {
        public Src()
        {
            var random = new Random();

            Name = Guid.NewGuid().ToString();
            Number = random.Next();
            Float = DateTime.Now.Millisecond / random.Next();
            DateTime = DateTime.Now;
        }

        public string Name { get; private set; }
        public int Number { get; private set; }
        public float Float { get; private set; }
        public DateTime DateTime { get; private set; }
    }

    public class Dest
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public float Float { get; set; }
        public DateTime DateTime { get; set; }
    }
}
