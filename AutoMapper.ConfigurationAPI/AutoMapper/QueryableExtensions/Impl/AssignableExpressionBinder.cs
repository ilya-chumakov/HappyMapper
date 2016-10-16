using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class AssignableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestType.IsAssignableFrom(result.Type) && propertyTypeMap == null;
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindAssignableExpression(propertyMap, result);
        }

        private static MemberAssignment BindAssignableExpression(PropertyMap propertyMap,
            ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestMember, result.ResolutionExpression);
        }
    }
}