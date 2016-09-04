using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.ConfigurationAPI.Execution;

namespace AutoMapper.ConfigurationAPI
{
    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, MemberInfo[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public MemberInfo[] SourceMembers { get; }

        public bool CanResolve { get; set; }

        public bool DefaultValue { get; private set; }

        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapPlanBuilder builder)
        {
            var valueResolverExpression = ResolveSource(builder.Source, builder.Context);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Expression.Variable(sourceType, "resolvedValue");            
            return Expression.Block(new[] { resolvedValue },
                Expression.Assign(resolvedValue, valueResolverExpression),
                builder.MapExpression(new TypePair(sourceType, DestinationType), resolvedValue));
        }

        private Expression ResolveSource(ParameterExpression sourceParameter, ParameterExpression contextParameter)
        {
            if(CustomExpression != null)
            {
                return CustomExpression.ConvertReplaceParameters(sourceParameter).IfNotNull(DestinationType);
            }
            if(CustomValueResolver != null)
            {
                // Invoking a delegate
                return Expression.Invoke(Expression.Constant(CustomValueResolver), sourceParameter, contextParameter);
            }
            if(Parameter.IsOptional)
            {
                DefaultValue = true;
                return Expression.Constant(Parameter.GetDefaultValue(), Parameter.ParameterType);
            }
            return SourceMembers.Aggregate(
                            (Expression) sourceParameter,
                            (inner, getter) => getter is MethodInfo
                                ? Expression.Call(getter.IsStatic() ? null : inner, (MethodInfo) getter)
                                : (Expression) Expression.MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                      ).IfNotNull(DestinationType);
        }
    }
}