using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class Recorder_Tests
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

        [Test]
        public void AssignAsNoCast_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsNoCast("A", "B", "P1", "P2");

            Assignment assignment = recorder.GetAssignment();

            string expected = "{1}.P2 = {0}.P1;\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
            Assert.AreEqual(expected.Apply("A", "B"), assignment.Code);
        }

        [Test]
        public void AssignAsExplicitCast_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsExplicitCast("A", "B", "P1", "P2", "ulong");

            Assignment assignment = recorder.GetAssignment();

            string expected = "{1}.P2 = (ulong) {0}.P1;\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
            Assert.AreEqual(expected.Apply("A", "B"), assignment.Code);
        }

        [Test]
        public void AssignAsToStringCall_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsToStringCall("A", "B", "P1", "P2");

            Assignment assignment = recorder.GetAssignment();

            string expected = "{1}.P2 = {0}.P1.ToString();\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
            Assert.AreEqual(expected.Apply("A", "B"), assignment.Code);
        }

        [Test]
        public void AssignAsStringToValueTypeConvert_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsStringToValueTypeConvert("A", "B", "P1", "P2", "ulong");

            Assignment assignment = recorder.GetAssignment();

            string expected = "{1}.P2 = (ulong) Convert.ChangeType({0}.P1, typeof(ulong));\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
            Assert.AreEqual(expected.Apply("A", "B"), assignment.Code);
        }
    }

    public static class StringEx
    {
        public static string Apply(this string template, string a, string b)
        {
            return string.Format(template, a, b);
        }
    }
}