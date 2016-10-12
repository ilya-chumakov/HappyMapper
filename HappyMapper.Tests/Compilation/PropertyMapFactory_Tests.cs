using AutoMapper.ConfigurationAPI;
using HappyMapper.Compilation;
using HappyMapper.Text;
using NUnit.Framework;

namespace HappyMapper.Tests.Compilation
{
    public class PropertyMapFactory_Tests
    {
        class A { public int P1 { get; set; } }
        class B { public int P1 { get; set; } }

        [Test]
        public void Create_ValidTypes_ReturnsNotNull()
        {
            var propertyMap = PropertyMapFactory.CreateReal(typeof(A), typeof(B), nameof(A.P1), nameof(B.P1));

            Assert.IsNotNull(propertyMap);
        }

        [Test]
        public void Create_ValidTypes_ContextCanBeCreated()
        {
            var propertyMap = PropertyMapFactory.CreateReal(typeof(A), typeof(B), nameof(A.P1), nameof(B.P1));

            var context = new PropertyNameContext(propertyMap);

            Assert.IsNotNull(context);
        }
    }
}