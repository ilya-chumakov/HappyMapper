using System;
using System.Linq.Expressions;

namespace AutoMapper.Extended.Net4
{
    public class OriginalStatement
    {
        public OriginalStatement(object @delegate, LambdaExpression expression)
        {
            Delegate = @delegate;
            Expression = expression;
        }

        public OriginalStatement(LambdaExpression expression)
        {
            Delegate = expression.Compile();
            Expression = expression;
        }

        /// <summary>
        /// Original compiled (to Func) condition.
        /// </summary>
        public object Delegate { get; private set; }
        /// <summary>
        /// Original expression condition (the Condition property stores a changed value).
        /// </summary>
        public LambdaExpression Expression { get; private set; }

        public string Id { get; } = NamingTools.NewGuid();
    }
}