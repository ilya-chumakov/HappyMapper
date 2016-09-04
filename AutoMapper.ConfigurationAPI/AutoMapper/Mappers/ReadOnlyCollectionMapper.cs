using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.ConfigurationAPI.Configuration;

namespace AutoMapper.ConfigurationAPI.Mappers
{
    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));
            var list = typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, Expression.Default(listType), contextExpression, _ => Expression.Constant(false), typeof(List<>), CollectionMapperExtensions.MapItemExpr);
            var dest = Expression.Variable(listType, "dest");

            return Expression.Block(new[] { dest }, Expression.Assign(dest, list), Expression.Condition(Expression.NotEqual(dest, Expression.Default(listType)), Expression.New(destExpression.Type.GetConstructors().First(), dest), Expression.Default(destExpression.Type)));
        }
    }
}