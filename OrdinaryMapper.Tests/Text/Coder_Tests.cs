using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class Coder_Tests
    {
        [Test]
        public void ShiftTemplate_Call_InsertsPropertyNames()
        {
            string template = "{1}.Child.Date = {0}.Child.Date;";
            string expected = "{1}.DestChild.Child.Date = {0}.SrcChild.Child.Date;";
            string srcName = "SrcChild";
            string destName = "DestChild";

            string actual = Recorder.ShiftTemplate(template, srcName, destName);

            Assert.AreEqual(expected, actual);
        }
    }
}