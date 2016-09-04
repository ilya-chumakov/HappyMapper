using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper.ConfigurationAPI.Execution
{
    public class DelegateFactory
    {
        private readonly ConcurrentDictionary<Type, LateBoundCtor> _ctorCache =
            new ConcurrentDictionary<Type, LateBoundCtor>();

        private readonly Func<Type, LateBoundCtor> _generateConstructor;

        public DelegateFactory()
        {
            _generateConstructor = GenerateConstructor;
        }

        public Expression<LateBoundMethod<object, TValue>> CreateGet<TValue>(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

            MethodCallExpression call;
            if (!method.IsDefined(typeof (ExtensionAttribute), false))
            {
                // instance member method
                call = Expression.Call(Expression.Convert(instanceParameter, method.DeclaringType), method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }
            else
            {
                // static extension method
                call = Expression.Call(
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }

            Expression<LateBoundMethod<object, TValue>> lambda = Expression.Lambda<LateBoundMethod<object, TValue>>(
                call,
                instanceParameter,
                argumentsParameter);

            return lambda;
        }

        public LateBoundCtor CreateCtor(Type type)
        {
            var ctor = _ctorCache.GetOrAdd(type, _generateConstructor);
            return ctor;
        }

        private static LateBoundCtor GenerateConstructor(Type type)
        {
            var ctorExpr = GenerateConstructorExpression(type);

            return Expression.Lambda<LateBoundCtor>(Expression.Convert(ctorExpr, typeof (object))).Compile();
        }

        public static Expression GenerateConstructorExpression(Type type)
        {
            if(type.IsValueType())
            {
                return Expression.Convert(Expression.New(type), typeof(object));
            }

            var constructors = type
                .GetDeclaredConstructors()
                .Where(ci => !ci.IsStatic);

            //find a ctor with only optional args
            var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
            if(ctorWithOptionalArgs == null)
            {
                var ex = new ArgumentException(type + " needs to have a constructor with 0 args or only optional args", "type");
                return Expression.Block(Expression.Throw(Expression.Constant(ex)), Expression.Constant(null));
            }
            //get all optional default values
            var args = ctorWithOptionalArgs
                .GetParameters()
                .Select(p => Expression.Constant(p.GetDefaultValue(), p.ParameterType)).ToArray();

            //create the ctor expression
            return Expression.New(ctorWithOptionalArgs, args);
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression instanceParameter,
            Expression argumentsParameter)
        {
            var expressions = new List<UnaryExpression>();
            var realMethodParameters = method.GetParameters();
            if (method.IsDefined(typeof (ExtensionAttribute), false))
            {
                Type extendedType = method.GetParameters()[0].ParameterType;
                expressions.Add(Expression.Convert(instanceParameter, extendedType));
                realMethodParameters = realMethodParameters.Skip(1).ToArray();
            }

            expressions.AddRange(realMethodParameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)));

            return expressions.ToArray();
        }
    }
}