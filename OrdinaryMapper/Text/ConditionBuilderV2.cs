using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ExpressionToCodeLib;

namespace OrdinaryMapper
{
    /// <summary>
    /// Captured variable support!
    /// </summary>
    public class ConditionBuilderV2 : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected PropertyNameContext Context { get; set; }
        protected Coder Coder { get; set; }

        public ConditionBuilderV2(PropertyNameContext context, Coder coder)
        {
            Context = context;
            Coder = coder;

            //var condition = Context.PropertyMap.Condition;
            var condition = Context.PropertyMap.OriginalCondition.Expression;
            //var x = Context.PropertyMap.Condition.Body as LambdaExpression;

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
}