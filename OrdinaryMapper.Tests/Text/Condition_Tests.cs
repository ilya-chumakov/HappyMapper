using System;
using System.Linq.Expressions;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.Text
{
    public class Condition_Tests
    {
        public class A { public int P1 { get; set; } }
        public class B { public int P1 { get; set; } }

        [Test]
        public void Condition_IsNotNull_PrintCode()
        {
            string srcFieldName = "x";
            string destFieldName = "y";

            var propertyMap = CreatePropertyMap<A, B>("P1", "P1");

            var context = new PropertyNameContext(propertyMap, srcFieldName, destFieldName);
            var coder = new Coder();

            Expression<Func<A, bool>> exp = src => src.P1 != 0;
            propertyMap.Condition = exp;

            using (var condition = new Condition(context, coder))
            {

            }

            Assert.IsNotNullOrEmpty(coder.GetAssignment().Code);
            Console.WriteLine(coder.GetAssignment().Code);
        }

        [Test]
        public void Condition_IsNull_NoCodeAppears()
        {
            string srcFieldName = "x";
            string destFieldName = "y";

            var propertyMap = CreatePropertyMap<A, B>("P1", "P1");

            var context = new PropertyNameContext(propertyMap, srcFieldName, destFieldName);
            var coder = new Coder();

            using (var condition = new Condition(context, coder)) { }

            Assert.IsNullOrEmpty(coder.GetAssignment().Code);
        }

        private static PropertyMap CreatePropertyMap<TSrc, TDest>(string srcPropertyName, string destPropertyName)
        {
            var factory = new TypeMapFactory();

            PropertyMap propertyMap = new PropertyMap(
                typeof (TDest).GetProperty(destPropertyName),
                factory.CreateTypeMap(typeof (TSrc), typeof (TDest),
                    new MapperConfigurationExpression()));

            propertyMap.ChainMembers(new[] {typeof (TSrc).GetProperty(srcPropertyName)});
            return propertyMap;
        }
    }
}