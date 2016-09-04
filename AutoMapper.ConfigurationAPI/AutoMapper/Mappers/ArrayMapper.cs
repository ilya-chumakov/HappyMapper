using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.ConfigurationAPI.Configuration;

namespace AutoMapper.ConfigurationAPI.Mappers
{
    public class ArrayMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);
        
        public static TDestination[] Map<TSource, TDestination>(IEnumerable<TSource> source, ResolutionContext context, Func<TSource, ResolutionContext, TDestination> newItemFunc)
        {
            var count = source.Count();
            var array = new TDestination[count];

            int i = 0;
            foreach (var item in source)
                array[i++] = newItemFunc(item, context);
            return array;
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            if (destExpression.Type.IsAssignableFrom(sourceExpression.Type) && configurationProvider.ResolveTypeMap(sourceElementType, destElementType) == null)
            {
                // return (TDestination[]) source;
                var convertExpr = Expression.Convert(sourceExpression, destElementType.MakeArrayType());

                if (configurationProvider.Configuration.AllowNullCollections)
                    return convertExpr;

                // return (TDestination[]) source ?? new TDestination[0];
                return Expression.Coalesce(convertExpr, Expression.NewArrayBounds(destElementType, Expression.Constant(0)));
            }

            var ifNullExpr = configurationProvider.Configuration.AllowNullCollections
                                 ? (Expression) Expression.Constant(null, destExpression.Type)
                                 : Expression.NewArrayBounds(destElementType, Expression.Constant(0));

            var itemParam = Expression.Parameter(sourceElementType, "item");
            var itemExpr = typeMapRegistry.MapItemExpr(configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type, itemParam, contextExpression);

            //var count = source.Count();
            //var array = new TDestination[count];

            //int i = 0;
            //foreach (var item in source)
            //    array[i++] = newItemFunc(item, context);
            //return array;

            var countParam = Expression.Parameter(typeof(int), "count");
            var arrayParam = Expression.Parameter(destExpression.Type, "destinationArray");
            var indexParam = Expression.Parameter(typeof(int), "destinationArrayIndex");

            var actions = new List<Expression>();
            var parameters = new List<ParameterExpression> { countParam, arrayParam, indexParam };

            var countMethod = typeof(Enumerable)
                .GetTypeInfo()
                .DeclaredMethods
                .Single(mi => mi.Name == "Count" && mi.GetParameters().Length == 1)
                .MakeGenericMethod(sourceElementType);
            actions.Add(Expression.Assign(countParam, Expression.Call(countMethod, sourceExpression)));
            actions.Add(Expression.Assign(arrayParam, Expression.NewArrayBounds(destElementType, countParam)));
            actions.Add(Expression.Assign(indexParam, Expression.Constant(0)));
            actions.Add(ExpressionExtensions.ForEach(sourceExpression, itemParam,
                Expression.Assign(Expression.ArrayAccess(arrayParam, Expression.PostIncrementAssign(indexParam)), itemExpr)
                ));
            actions.Add(arrayParam);

            var mapExpr = Expression.Block(parameters, actions);

            // return (source == null) ? ifNullExpr : Map<TSourceElement, TDestElement>(source, context);
            return Expression.Condition(Expression.Equal(sourceExpression, Expression.Constant(null)), ifNullExpr, mapExpr);
        }

    }
}