using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OrdinaryMapper
{
    public class Condition : IDisposable
    {
        public Condition(PropertyNameContext context, Coder coder)
        {
            Context = context;
            Coder = coder;

            var condition = Context.PropertyMap.Condition;

            IsExist = condition != null;

            if (IsExist)
            {
                Coder.AttachRawCode("{{");
            }
        }

        public bool IsExist { get; set; } = false;

        protected PropertyNameContext Context { get; set; }
        protected Coder Coder { get; set; }

        public void Dispose()
        {
            if (IsExist)
            {
                Coder.AttachRawCode("}}");
            }
        }
    }

    public class ParameterNameReplaceVisitor : ExpressionVisitor
    {
        public Type SrcType { get; set; }
        public Type DestType { get; set; }
        public string SrcName { get; set; }
        public string DestName { get; set; }
        public ICollection<ParameterExpression> Parameters { get; set; }

        public ParameterNameReplaceVisitor(Type srcType, Type destType, string srcName, string destName, ICollection<ParameterExpression> parameters)
        {
            SrcType = srcType;
            DestType = destType;
            SrcName = srcName;
            DestName = destName;
            Parameters = parameters;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (Parameters.Count == 0) return node;

            if (Parameters.Count == 1)
            {
                if (node.Type == SrcType) return Expression.Parameter(SrcType, SrcName);
                if (node.Type == DestType) return Expression.Parameter(DestType, DestName);
            }

            if (Parameters.Count == 2)
            {
                if (node.Type == SrcType || node.Name == Parameters.First().Name)
                {
                    return Expression.Parameter(SrcType, SrcName);
                }
                if (node.Type == DestType || node.Name == Parameters.Last().Name)
                {
                    return Expression.Parameter(DestType, DestName);
                }
            }

            return node;
        }
    }
}