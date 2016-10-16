using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI.QueryableExtensions
{
    public interface IExpressionBinder
    {
        bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result);

        MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount);
    }
}