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
        public void SimpleAssign_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsNoCast("A", "B", "P1", "P2");

            Assignment assignment = recorder.GetAssignment();

            Assert.AreEqual("B.P2 = A.P1;\r\n", assignment.Code);

            Assert.AreEqual("{1}.P2 = {0}.P1;\r\n", assignment.RelativeTemplate);
        }

        [Test]
        public void ExplicitCastAssign_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsExplicitCast("A", "B", "P1", "P2", "ulong");

            Assignment assignment = recorder.GetAssignment();

            Assert.AreEqual("B.P2 = (ulong) A.P1;\r\n", assignment.Code);

            Assert.AreEqual("{1}.P2 = (ulong) {0}.P1;\r\n", assignment.RelativeTemplate);
        }

        [Test]
        public void ToStringAssign_Call_ReturnsValidPropertyAssignment()
        {
            Recorder recorder = new Recorder();

            recorder.AssignAsToStringCall("A", "B", "P1", "P2");

            Assignment assignment = recorder.GetAssignment();

            Assert.AreEqual("B.P2 = A.P1.ToString();\r\n", assignment.Code);

            Assert.AreEqual("{1}.P2 = {0}.P1.ToString();\r\n", assignment.RelativeTemplate);
        }
    }
}