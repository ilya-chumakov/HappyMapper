using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Extended.Net4;
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

            var condition = Context.PropertyMap.OriginalCondition;

            IsExist = condition != null;

            if (IsExist)
            {
                string text = ToCode(condition);
                string template = ToTemplate(condition);

                Coder.AppendLine($"if ({text})", $"if ({template})");
                Coder.AttachRawCode("{{");
            }
        }

        private string ToCode(OriginalCondition condition)
        {
            string id = condition.Id;
            string methodName = $"OrdinaryMapper.ConditionStore.Condition_{id}";

            string methodCall = $"{methodName}({Context.SrcMemberPrefix}, {Context.DestMemberPrefix})";

            return methodCall;
        }

        private string ToTemplate(OriginalCondition condition)
        {
            string id = condition.Id;
            string methodName = $"OrdinaryMapper.ConditionStore.Condition_{id}";

            string methodCall = $"{methodName}({{0}}, {{1}})";

            return methodCall;
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