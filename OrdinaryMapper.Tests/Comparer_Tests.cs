using System;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class Comparer_Tests
    {
        [Test]
        public void AreEqual_WhenComparesEqualObjects_ReturnsSuccess()
        {
            Src src = new Src();
            Dest dest = new Dest();

            dest.Name = src.Name;
            dest.Number = src.Number;
            dest.Float = src.Float;
            dest.DateTime = src.DateTime;

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);

            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Errors);
        } 
    }
}