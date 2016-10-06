using NUnit.Framework;

namespace HappyMapper.Tests.Issues
{
    public class DestWithoutParameterlessCtor_Tests
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
            public DestChild_NoCtor Child { get; set; }
        }

        public class DestChild_NoCtor
        {
            public DestChild_NoCtor(int val)
            {
                MyProperty = val;
            }
            public int MyProperty { get; set; }
        }

        [Test]
        public void CompileMapper_NoDestChildCtor_ThrowsEx()
        {
            Assert.Throws(Is.TypeOf<HappyMapperException>()
                         .And.Message.ContainsSubstring("parameterless ctor"),
                delegate
                {
                    var config = new HappyConfig(cfg =>
                    {
                        cfg.CreateMap<Src, Dest>();
                    });
                    var mapper = config.CompileMapper();

                    //var src = new Src();
                    //var dest = new Dest();

                    //mapper.Map(src, dest);
                });
        }
    }
}