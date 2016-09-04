using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.ConfigurationAPI
{
    internal static class ExpressionExtensions
    {
        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            if(collection.Type.IsArray)
            {
                return ForEachArrayItem(collection, arrayItem => Expression.Block(new[] { loopVar }, Expression.Assign(loopVar, arrayItem), loopContent));
            }
            var elementType = loopVar.Type;
            var getEnumerator = collection.Type.GetDeclaredMethod("GetEnumerator") ?? 
                                           typeof(IEnumerable<>).MakeGenericType(elementType).GetDeclaredMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(collection, getEnumerator);
            var enumeratorType = getEnumeratorCall.Type;
            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNext = enumeratorType.GetDeclaredMethod("MoveNext") ?? typeof(IEnumerator).GetDeclaredMethod("MoveNext");
            var moveNextCall = Expression.Call(enumeratorVar, moveNext);

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static Expression ForEachArrayItem(Expression array, Func<Expression, Expression> body)
        {
            var length = Expression.Property(array, "Length");
            return For(length, index => body(Expression.ArrayAccess(array, index)));
        }

        public static Expression For(Expression count, Func<Expression, Expression> body)
        {
            var breakLabel = Expression.Label("LoopBreak");
            var index = Expression.Variable(typeof(int), "sourceArrayIndex");
            var initialize = Expression.Assign(index, Expression.Constant(0, typeof(int)));
            var loop = Expression.Block(new[] { index },
                initialize,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(index, count),
                        Expression.Block(body(index), Expression.PostIncrementAssign(index)),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );
            return loop;
        }

        public static Expression ToObject(Expression expression)
        {
            return expression.Type == typeof(object) ? expression : Expression.Convert(expression, typeof(object));
        }

        public static Expression ToType(Expression expression, Type type)
        {
            return expression.Type == type ? expression : Expression.Convert(expression, type);
        }

        public static Expression ConsoleWriteLine(string value, params Expression[] values)
        {
            return Expression.Call(typeof (Debug).GetDeclaredMethod("WriteLine", new[] {typeof (string), typeof(object[])}), 
                Expression.Constant(value), 
                Expression.NewArrayInit(typeof(object), values.Select(ToObject).ToArray()));
        }

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = replaceExp.Replace(exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Count(), exp.Parameters.Count()); i++)
                replaceExp = new ConvertingVisitor(exp.Parameters[i], replace[i]).Visit(replaceExp);
            return replaceExp;
        }

        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat) => (LambdaExpression)new ExpressionConcatVisitor(expr).Visit(concat);

        public static Expression IfNotNull(this Expression expression, Type destinationType)
        {
            var node = expression;
            var isMemberAccess = node.NodeType == ExpressionType.MemberAccess || (node.NodeType == ExpressionType.Call && ((MethodCallExpression)node).Arguments.Count == 0);
            while (isMemberAccess)
            {
                node = (node as MemberExpression)?.Expression 
                    ?? (node as MethodCallExpression)?.Object;
                isMemberAccess =
                    (node != null) && (
                    node.NodeType == ExpressionType.MemberAccess 
                    || (node.NodeType == ExpressionType.Call && ((MethodCallExpression)node).Arguments.Count == 0));
            }
            if(node != null && node.NodeType == ExpressionType.Parameter)
            {
                return new IfNotNullVisitor().VisitRoot(expression, destinationType);
            }
            return expression;
        }

        public static Expression RemoveIfNotNull(this Expression expression, params Expression[] expressions) => new RemoveIfNotNullVisitor(expressions).Visit(expression);

        public static Expression IfNullElse(this Expression expression, params Expression[] ifElse)
        {
            return ifElse.Any()
                ? Expression.Condition(Expression.NotEqual(expression, Expression.Default(expression.Type)), expression, ifElse.First().IfNullElse(ifElse.Skip(1).ToArray()))
                : expression;
        }

        internal class IfNotNullVisitor : ExpressionVisitor
        {
            private Expression nullConditions = Expression.Constant(false);

            public Expression VisitRoot(Expression node, Type destinationType)
            {
                var returnType = Nullable.GetUnderlyingType(destinationType) == node.Type ? destinationType : node.Type;
                var expression = base.Visit(node);
                var checkNull = Expression.Condition(nullConditions, Expression.Default(returnType), ToType(expression, returnType));
                return checkNull;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var returnNode = base.VisitMember(node);
                if(node.Expression == null || node.Expression.Type.IsValueType())
                {
                    return returnNode;
                }
                nullConditions = Expression.OrElse(nullConditions, Expression.Equal(node.Expression, Expression.Constant(null, node.Expression.Type)));
                return returnNode;
            }
        }

        internal class RemoveIfNotNullVisitor : ExpressionVisitor
        {
            private readonly Expression[] _expressions;
            private readonly IList<Expression> AllreadyUpdated = new List<Expression>();

            public RemoveIfNotNullVisitor(params Expression[] expressions)
            {
                _expressions = expressions;
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                var member = node.IfFalse as MemberExpression;
                var binary = node.Test as BinaryExpression;
                if(member == null || binary == null || !_expressions.Contains(binary.Left) || !(binary.Right is DefaultExpression))
                    return base.VisitConditional(node);
                return node.IfFalse;
            }
        }

        internal class ConvertingVisitor : ExpressionVisitor
        {
            private readonly Expression _newParam;
            private readonly ParameterExpression _oldParam;

            public ConvertingVisitor(ParameterExpression oldParam, Expression newParam)
            {
                _newParam = newParam;
                _oldParam = oldParam;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return node.Expression == _oldParam
                    ? Expression.MakeMemberAccess(ToType(_newParam, _oldParam.Type), node.Member)
                    : base.VisitMember(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? ToType(_newParam, _oldParam.Type) : base.VisitParameter(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Object == _oldParam
                    ? Expression.Call(ToType(_newParam, _oldParam.Type), node.Method, node.Arguments)
                    : base.VisitMethodCall(node);
            }
        }

        internal class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldExpression;
            private readonly Expression _newExpression;

            public ReplaceExpressionVisitor(Expression oldExpression, Expression newExpression)
            {
                _oldExpression = oldExpression;
                _newExpression = newExpression;
            }

            public override Expression Visit(Expression node)
            {
                return _oldExpression == node ? _newExpression : base.Visit(node);
            }
        }

        internal class ExpressionConcatVisitor : ExpressionVisitor
        {
            private readonly LambdaExpression _overrideExpression;

            public ExpressionConcatVisitor(LambdaExpression overrideExpression)
            {
                _overrideExpression = overrideExpression;
            }

            public override Expression Visit(Expression node)
            {
                if (_overrideExpression == null)
                    return node;
                if (node.NodeType != ExpressionType.Lambda && node.NodeType != ExpressionType.Parameter)
                {
                    var expression = node;
                    if (node.Type == typeof(object))
                        expression = Expression.Convert(node, _overrideExpression.Parameters[0].Type);

                    return _overrideExpression.ReplaceParameters(expression);
                }
                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda(Visit(node.Body), node.Parameters);
            }
        }

    }
}