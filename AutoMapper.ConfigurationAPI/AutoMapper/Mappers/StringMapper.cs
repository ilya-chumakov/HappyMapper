using System.Linq.Expressions;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Mappers
{
    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            return Expression.Condition(Expression.Equal(sourceExpression, Expression.Default(sourceExpression.Type)),
                Expression.Constant(null, typeof(string)),
                Expression.Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString")));
        }
    }
}