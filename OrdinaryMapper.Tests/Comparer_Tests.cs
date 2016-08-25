using System;
using NUnit.Framework;
using OrdinaryMapper.Benchmarks;
using OrdinaryMapper.Benchmarks.Types;
using OrdinaryMapper.Tests.Tools;

namespace OrdinaryMapper.Tests
{
    public class Comparer_Tests
    {
        [Test]
        public void AreEqual_EqualSimpleObjects_ReturnsTrue()
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

        [Test]
        public void AreEqual_NotEqualSimpleObjects_ReturnsFalse()
        {
            Src src = new Src();
            Dest dest = new Dest();

            dest.Name = src.Name;
            dest.Number = src.Number;
            dest.Float = src.Float;
            dest.Float = src.Float + 0.1f;
            dest.DateTime = src.DateTime;

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);

            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.Errors);
        }


        [Test]
        public void AreEqual_EqualNestedObjects_ReturnsTrue()
        {
            var src = new NestedSrc();
            var dest = new NestedDest();

            dest.Name = src.Name;
            dest.Number = src.Number;
            dest.Float = src.Float;
            dest.DateTime = src.DateTime;
            dest.Child.MyProperty = src.Child.MyProperty;
            dest.Child.GrandChild.Foo = src.Child.GrandChild.Foo;

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);

            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Errors);
        }

        [Test]
        public void AreEqual_NotEqualNestedObjects_ReturnsFalse()
        {
            var src = new NestedSrc();
            var dest = new NestedDest();

            dest.Name = src.Name;
            dest.Number = src.Number;
            dest.Float = src.Float;
            dest.DateTime = src.DateTime;
            dest.Child.MyProperty = src.Child.MyProperty - 42;

            var result = ObjectComparer.AreEqual(src, dest);

            result.Errors.ForEach(Console.WriteLine);

            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.Errors);
        }

    }
}