using HappyMapper.Text;
using NUnit.Framework;

namespace HappyMapper.Tests.Text
{
    public class FakeAssignContext : IAssignContext
    {
        public string DestMemberPrefix { get; }
        public string DestMemberName { get; }
        public string SrcMemberName { get; }
        public string SrcMemberPrefix { get; }
        public string DestTypeFullName { get; set; }

        public FakeAssignContext(string srcMemberPrefix, string destMemberPrefix, string srcMemberName, string destMemberName, string destTypeFullName)
        {
            DestMemberPrefix = destMemberPrefix;
            DestMemberName = destMemberName;
            SrcMemberName = srcMemberName;
            SrcMemberPrefix = srcMemberPrefix;
            DestTypeFullName = destTypeFullName;
        }
    }

    public class Recorder_Tests
    {
        public FakeAssignContext Context { get; set; }

        [SetUp]
        public void SetUp()
        {
            Context = new FakeAssignContext("A", "B", "P1", "P2", "ulong");
        }

        [Test]
        public void ShiftTemplate_Call_InsertsPropertyNames()
        {
            string template = "{1}.Child.Date = {0}.Child.Date;";
            string expected = "{1}.DestChild.Child.Date = {0}.SrcChild.Child.Date;";
            string srcName = "SrcChild";
            string destName = "DestChild";

            string actual = template.AddPropertyNamesToTemplate(srcName, destName);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AssignAsNoCast_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AppendAssignment(Assign.AsNoCast, Context);

            Assignment assignment = recorder.ToAssignment();

            string expected = "{1}.P2 = {0}.P1;\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
        }

        [Test]
        public void AssignAsExplicitCast_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AppendAssignment(Assign.AsExplicitCast, Context);

            Assignment assignment = recorder.ToAssignment();

            string expected = "{1}.P2 = (ulong) {0}.P1;\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
        }

        [Test]
        public void AssignAsToStringCall_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AppendAssignment(Assign.AsToStringCall, Context);

            Assignment assignment = recorder.ToAssignment();

            string expected = "{1}.P2 = {0}.P1.ToString();\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
        }

        [Test]
        public void AssignAsStringToValueTypeConvert_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AppendAssignment(Assign.AsStringToValueTypeConvert, Context);

            Assignment assignment = recorder.ToAssignment();

            string expected = "{1}.P2 = (ulong) Convert.ChangeType({0}.P1, typeof(ulong));\r\n";
            Assert.AreEqual(expected, assignment.RelativeTemplate);
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