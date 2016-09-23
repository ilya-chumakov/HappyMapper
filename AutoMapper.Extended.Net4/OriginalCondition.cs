using System;
using System.Linq.Expressions;

namespace AutoMapper.Extended.Net4
{
    public class OriginalCondition
    {
        public OriginalCondition(object fnc, LambdaExpression expression)
        {
            Fnc = fnc;
            Expression = expression;
        }

        /// <summary>
        /// Original compiled (to Func) condition.
        /// </summary>
        public object Fnc { get; private set; }
        /// <summary>
        /// Original expression condition (the Condition property stores a changed value).
        /// </summary>
        public LambdaExpression Expression { get; private set; }

        public string Id { get; } = Guid.NewGuid().ToString().Replace("-", "");
    }
}