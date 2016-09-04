using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestType == typeof(string);
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindStringExpression(propertyMap, result);
        }

        private static MemberAssignment BindStringExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestMember, Expression.Call(result.ResolutionExpression, "ToString", null, null));
        }
    }
}