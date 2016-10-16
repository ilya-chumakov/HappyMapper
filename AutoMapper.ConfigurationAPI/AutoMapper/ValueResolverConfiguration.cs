using System;
using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI
{
    public class ValueResolverConfiguration
    {
        public object Instance { get; }
        public Type Type { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }

        public ValueResolverConfiguration(Type type)
        {
            Type = type;
        }

        public ValueResolverConfiguration(object instance)
        {
            Instance = instance;
        }
    }
}