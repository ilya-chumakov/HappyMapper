using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class CustomProjectionExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap?.CustomProjection != null;
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindCustomProjectionExpression(propertyMap, propertyTypeMap, result);
        }

        private static MemberAssignment BindCustomProjectionExpression(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestMember, propertyTypeMap.CustomProjection.ConvertReplaceParameters(result.ResolutionExpression));
        }
    }
}