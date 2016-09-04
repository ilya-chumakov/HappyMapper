using System.Collections.Concurrent;
using System.Linq.Expressions;
using AutoMapper.ConfigurationAPI.Configuration;

namespace AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class NullableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestType.IsNullableType()
                   && !result.Type.IsNullableType();
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindNullableExpression(propertyMap, result);
        }

        private static MemberAssignment BindNullableExpression(PropertyMap propertyMap,
            ExpressionResolutionResult result)
        {
            if (result.ResolutionExpression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) result.ResolutionExpression;
                if (memberExpr.Expression != null && memberExpr.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    var destType = propertyMap.DestType;
                    var parentExpr = memberExpr.Expression;
                    Expression expressionToBind = Expression.Convert(memberExpr, destType);
                    var nullExpression = Expression.Convert(Expression.Constant(null), destType);
                    while (parentExpr.NodeType != ExpressionType.Parameter)
                    {
                        memberExpr = (MemberExpression) memberExpr.Expression;
                        parentExpr = memberExpr.Expression;
                        expressionToBind = Expression.Condition(
                            Expression.Equal(memberExpr, Expression.Constant(null)),
                            nullExpression,
                            expressionToBind
                            );
                    }

                    return Expression.Bind(propertyMap.DestMember, expressionToBind);
                }
            }

            return Expression.Bind(propertyMap.DestMember,
                Expression.Convert(result.ResolutionExpression, propertyMap.DestType));
        }
    }
}