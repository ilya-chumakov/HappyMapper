using System;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    /// <summary>
    /// Captured variable support!
    /// </summary>
    public class ConditionPrinterV2 : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected PropertyNameContext Context { get; set; }
        protected Coder Coder { get; set; }
        public ActionNameConvention NameConvention { get; set; }

        public ConditionPrinterV2(PropertyNameContext context, Coder coder)
        {
            NameConvention = NameConventions.Condition;
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

        private string ToCode(OriginalStatement condition)
        {
            string methodCall = ToTemplate(condition).TemplateToCode(Context.SrcMemberPrefix, Context.DestMemberPrefix);

            return methodCall;
        }

        private string ToTemplate(OriginalStatement condition)
        {
            string id = condition.Id;
            string memberName = NameConvention.GetMemberFullName(condition.Id);

            string call = $"{memberName}({{0}}, {{1}})";

            return call;
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