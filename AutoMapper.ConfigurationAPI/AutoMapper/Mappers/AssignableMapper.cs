using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Mappers
{
    public class AssignableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return sourceExpression;
        }
    }
}