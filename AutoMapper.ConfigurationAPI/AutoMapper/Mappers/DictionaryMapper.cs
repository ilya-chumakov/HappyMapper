using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.ConfigurationAPI.Configuration;

namespace AutoMapper.ConfigurationAPI.Mappers
{
    public class DictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType();

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }
}