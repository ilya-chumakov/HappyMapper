using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    /// <summary>
    /// Captured variable support!
    /// </summary>
    public class BeforeMapActionBuilder : IDisposable
    {
        private string srcFieldName;
        private string destFieldName;

        public bool IsExist { get; set; } = false;
        protected TypeMap TypeMap { get; set; }
        protected Coder Coder { get; set; }
        public BeforeMapActionNameConvention NameConvention { get; set; }

        public BeforeMapActionBuilder(TypeMap typeMap, Coder coder, string srcFieldName, string destFieldName)
        {
            NameConvention = NameConventionConfig.BeforeMapActionNameConvention;

            this.srcFieldName = srcFieldName;
            this.destFieldName = destFieldName;

            TypeMap = typeMap;
            Coder = coder;

            var statements = TypeMap.BeforeMapStatements;

            IsExist = statements.Any();

            if (IsExist)
            {
                List<string> texts = new List<string>();
                List<string> templates = new List<string>();

                foreach (var statement in statements)
                {
                    texts.Add(ToCode(statement));
                    templates.Add(ToTemplate(statement));
                }

                string text = string.Join("", texts);
                string template = string.Join("", texts);

                Coder.AttachRawCode("{{ ");

                Coder.AppendLine(text, text);
            }
        }

        private string ToCode(OriginalStatement condition)
        {
            //string id = condition.Id;
            //string methodName = $"OrdinaryMapper.BeforeMapActionStore.BeforeMapAction_{id}";
            //string context = $"new {typeof (ResolutionContext).FullName}()";

            //string methodCall = $"{methodName}({srcFieldName}, {destFieldName}, {context});";

            string methodCall = ToTemplate(condition)
                .Replace("{0}", srcFieldName)
                .Replace("{1}", destFieldName);

            return methodCall;
        }

        private string ToTemplate(OriginalStatement condition)
        {
            string methodName = NameConvention.GetMemberFullName(condition.Id);
            string context = $"new {typeof(ResolutionContext).FullName}()";

            string methodCall = $"{methodName}({{0}}, {{1}}, {context});";

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