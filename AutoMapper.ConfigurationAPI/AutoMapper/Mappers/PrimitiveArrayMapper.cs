using System;
using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Mappers
{
    public class PrimitiveArrayMapper : IObjectMapper
    {
        private bool IsPrimitiveArrayType(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = TypeHelper.GetElementType(type);
                return elementType.IsPrimitive() || elementType == typeof (string);
            }

            return false;
        }

        public bool IsMatch(TypePair context)
        {
            return IsPrimitiveArrayType(context.DestinationType) &&
                   IsPrimitiveArrayType(context.SourceType) &&
                   (TypeHelper.GetElementType(context.DestinationType) == TypeHelper.GetElementType(context.SourceType));
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Type destElementType = TypeHelper.GetElementType(destExpression.Type);

            Expression<Action> expr = () => Array.Copy(null, null, 0);
            var copyMethod = ((MethodCallExpression) expr.Body).Method;

            var valueIfNullExpr = configurationProvider.Configuration.AllowNullCollections
                ? (Expression) Expression.Constant(null, destExpression.Type)
                : Expression.NewArrayBounds(destElementType, Expression.Constant(0));

            var dest = Expression.Parameter(destExpression.Type, "destArray");
            var sourceLength = Expression.Parameter(typeof(int), "sourceLength");
            var lengthProperty = typeof(Array).GetDeclaredProperty("Length");
            var mapExpr = Expression.Block(
                new[] {dest, sourceLength},
                Expression.Assign(sourceLength, Expression.Property(sourceExpression, lengthProperty)),
                Expression.Assign(dest, Expression.NewArrayBounds(destElementType, sourceLength)),
                Expression.Call(copyMethod, sourceExpression, dest, sourceLength),
                dest
            );

            return Expression.Condition(Expression.Equal(sourceExpression, Expression.Constant(null)), valueIfNullExpr, mapExpr);
        }
    }
}