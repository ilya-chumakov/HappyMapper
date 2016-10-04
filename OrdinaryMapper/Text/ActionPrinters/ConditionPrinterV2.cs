using System;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper.Text
{
    /// <summary>
    /// Captured variable support!
    /// </summary>
    public class ConditionPrinterV2 : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected PropertyNameContext Context { get; set; }
        protected Recorder Recorder { get; set; }
        public ActionNameConvention NameConvention { get; set; }

        public ConditionPrinterV2(PropertyNameContext context, Recorder recorder)
        {
            NameConvention = NameConventions.Condition;
            Context = context;
            Recorder = recorder;

            var condition = Context.PropertyMap.OriginalCondition;

            IsExist = condition != null;

            if (IsExist)
            {
                string template = ToTemplate(condition);

                Recorder.AppendLine($"if ({template})");
                Recorder.AppendRawCode("{{");
            }
        }

        private string ToTemplate(OriginalStatement condition)
        {
            string memberName = NameConvention.GetMemberFullName(condition.Id);

            string call = $"{memberName}({{0}}, {{1}})";

            return call;
        }

        public void Dispose()
        {
            if (IsExist)
            {
                Recorder.AppendRawCode("}}");
            }
        }
    }
}