using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ExpressionToCodeLib;

namespace OrdinaryMapper
{
    /// <summary>
    /// Only src and dest - no captured variables!
    /// </summary>
    [Obsolete]
    public class ConditionPrinter : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected PropertyNameContext Context { get; set; }
        protected Coder Coder { get; set; }

        public ConditionPrinter(PropertyNameContext context, Coder coder)
        {
            Context = context;
            Coder = coder;

            var condition = Context.PropertyMap?.OriginalCondition?.Expression;

            IsExist = condition != null;

            if (IsExist)
            {
                string text = ToCode(condition);
                string template = ToTemplate(condition);

                Coder.AppendLine($"if ({text})", $"if ({template})");
                Coder.AttachRawCode("{{");
            }
        }

        private string ToCode(LambdaExpression condition)
        {
            var visitor = new ParameterNameReplaceVisitor(
                Context.PropertyMap.TypeMap.SourceType,
                Context.PropertyMap.TypeMap.DestinationType,
                Context.SrcMemberPrefix,
                Context.DestMemberPrefix,
                condition.Parameters);

            var visited = visitor.Visit(condition);

            var modifiedCondition = visited as LambdaExpression;

            return ExpressionToCode.ToCode(modifiedCondition.Body);
        }

        private string ToTemplate(LambdaExpression condition)
        {
            var visitor = new ParameterNameReplaceVisitor(
                Context.PropertyMap.TypeMap.SourceType,
                Context.PropertyMap.TypeMap.DestinationType,
                "{0}",
                "{1}",
                condition.Parameters);

            var modifiedCondition = visitor.Visit(condition) as LambdaExpression;

            return ExpressionToCode.ToCode(modifiedCondition.Body);
        }

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

        public ParameterNameReplaceVisitor(
            Type srcType, Type destType, 
            string srcName, string destName, 
            ReadOnlyCollection<ParameterExpression> parameters)
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
                if (node.Type == SrcType || node.Name == (Parameters.First() as ParameterExpression).Name)
                {
                    return Expression.Parameter(SrcType, SrcName);
                }
                if (node.Type == DestType || node.Name == (Parameters.Last() as ParameterExpression).Name)
                {
                    return Expression.Parameter(DestType, DestName);
                }
            }

            return node;
        }
    }
}