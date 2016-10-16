using System.Collections.Generic;
using System.Linq.Expressions;
using HappyMapper.AutoMapper.ConfigurationAPI.Configuration;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Mappers
{
    public class EnumerableToDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType.IsDictionaryType()
                   && context.SourceType.IsEnumerableType()
                   && !context.SourceType.IsDictionaryType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
                PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
                Expression contextExpression)
            =>
            typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression,
                contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>),
                CollectionMapperExtensions.MapItemExpr);
    }
}