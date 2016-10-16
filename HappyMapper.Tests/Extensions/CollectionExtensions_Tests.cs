using System.Collections.Generic;
using AutoMapper.Extended.Net4.SharedTools;
using HappyMapper.Benchmarks.Types;
using NUnit.Framework;

namespace HappyMapper.Tests
{
    public class CollectionExtensions_Tests
    {
        [Test]
        public void FillStrings_Test()
        {
            var list = new List<string>();

            int expected = 10;

            list.Add(expected, () => null);

            Assert.AreEqual(expected, list.Count);
        }

        [Test]
        public void FillValueTypes_Test()
        {
            FillValueTypes<int>();
        }

        private static void FillValueTypes<T>()
        {
            var list = new List<T>();

            int expected = 10;

            list.Add(expected, () => default(T));

            Assert.AreEqual(expected, list.Count);
        }

        [Test]
        public void FillReferenceTypes_Test()
        {
            FillReferenceTypes<Src>();
        }

        private static void FillReferenceTypes<T>() where T : class, new()
        {
            var list = new List<T>();

            int expected = 10;

            list.Add(expected, () => new T());

            Assert.AreEqual(expected, list.Count);
        }

    }
}