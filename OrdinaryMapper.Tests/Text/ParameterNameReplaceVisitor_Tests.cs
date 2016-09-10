using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class ParameterNameReplaceVisitor_Tests
    {
        public class Src { public string P1 { get; set; } }
        public class Dest { public string P2 { get; set; } }

        [Test]
        public void Visit_BodyContainsBothParameters_ReplaceBoth()
        {
            Expression<Func<Src, Dest, bool>> exp = (src, dest) => src.P1 + dest.P2 != null;

            var v = new ParameterNameReplaceVisitor(
                typeof(Src), typeof(Dest), "new_src", "new_dest", exp.Parameters);

            exp = v.Visit(exp) as Expression<Func<Src, Dest, bool>>;

            string expected = "(new_src, new_dest) => ((new_src.P1 + new_dest.P2) != null)";

            Assert.AreEqual(expected, exp.ToString());
        }

        [Test]
        public void Visit_BodyContainsSrc_ReplaceSrc()
        {
            Expression<Func<Src, Dest, bool>> exp = (src, dest) => src.P1 != null;

            var v = new ParameterNameReplaceVisitor(
                typeof(Src), typeof(Dest), "new_src", "new_dest", exp.Parameters);

            exp = v.Visit(exp) as Expression<Func<Src, Dest, bool>>;

            string expected = "(new_src, new_dest) => (new_src.P1 != null)";

            Assert.AreEqual(expected, exp.ToString());
        }

        [Test]
        public void Visit_BodyContainsDest_ReplaceDest()
        {
            Expression<Func<Src, Dest, bool>> exp = (src, dest) => dest.P2 != null;

            var v = new ParameterNameReplaceVisitor(
                typeof(Src), typeof(Dest), "new_src", "new_dest", exp.Parameters);

            exp = v.Visit(exp) as Expression<Func<Src, Dest, bool>>;

            string expected = "(new_src, new_dest) => (new_dest.P2 != null)";

            Assert.AreEqual(expected, exp.ToString());
        }
    }
}