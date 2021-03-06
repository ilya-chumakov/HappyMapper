using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ExpressionToCodeLib;

namespace HappyMapper.Text
{
    /// <summary>
    /// Only src and dest - no captured variables!
    /// </summary>
    [Obsolete]
    internal class ConditionPrinter : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected PropertyNameContext Context { get; set; }
        protected Recorder Recorder { get; set; }

        public ConditionPrinter(PropertyNameContext context, Recorder recorder)
        {
            Context = context;
            Recorder = recorder;

            var condition = Context.PropertyMap?.OriginalCondition?.Expression;

            IsExist = condition != null;

            if (IsExist)
            {
                string template = ToTemplate(condition);

                Recorder.AppendLine($"if ({template})");
                Recorder.AppendLine("{{");
            }
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
                Recorder.AppendLine("}}");
            }
        }
    }

    internal class ParameterNameReplaceVisitor : ExpressionVisitor
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