using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace HappyMapper.Tests
{
    public class TypeExtensions_Tests
    {
        [Test]
        public void GetFriendlyName()
        {
            Assert.AreEqual("System.String", typeof (string).GetFriendlyName());
            Assert.AreEqual("System.Int32[]", typeof (int[]).GetFriendlyName());
            Assert.AreEqual("System.Int32[][]", typeof (int[][]).GetFriendlyName());

            Assert.AreEqual("System.Collections.Generic.KeyValuePair<System.Int32,System.String>",
                typeof (KeyValuePair<int, string>).GetFriendlyName());

            Assert.AreEqual("System.Tuple<System.Int32,System.String>", typeof (Tuple<int, string>).GetFriendlyName());

            Assert.AreEqual(
                "System.Tuple<System.Collections.Generic.KeyValuePair<System.Object,System.Int64>,System.String>",
                typeof (Tuple<KeyValuePair<object, long>, string>).GetFriendlyName());

            Assert.AreEqual("System.Collections.Generic.List<System.Tuple<System.Int32,System.String>>",
                typeof (List<Tuple<int, string>>).GetFriendlyName());

            Assert.AreEqual("System.Tuple<System.Int16[],System.String>",
                typeof (Tuple<short[], string>).GetFriendlyName());
        }
    }
}