using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using HappyMapper.AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.AutoMapper.ConfigurationAPI.Mappers;

namespace HappyMapper.AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestType.IsEnumerableType() && propertyMap.SrcType.IsEnumerableType() &&
                    !(TypeHelper.GetElementType(propertyMap.DestType).IsPrimitive() && TypeHelper.GetElementType(propertyMap.SrcType).IsPrimitive());
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindEnumerableExpression(configuration, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindEnumerableExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            var destinationListType = TypeHelper.GetElementType(propertyMap.DestType);
            var sourceListType = TypeHelper.GetElementType(propertyMap.SrcType);
            var expression = result.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand);
                var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount);
                if(transformedExpression == null)
                {
                    return null;
                }
                expression = Expression.Call(typeof (Enumerable), "Select", new[] {sourceListType, destinationListType}, result.ResolutionExpression, transformedExpression);
            }

            expression = Expression.Call(typeof(Enumerable), propertyMap.DestType.IsArray ? "ToArray" : "ToList", new[] { destinationListType }, expression);

            return Expression.Bind(propertyMap.DestMember, expression);
        }
    }
}