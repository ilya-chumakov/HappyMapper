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
    public class BeforeMapPrinter : IDisposable
    {
        public bool IsExist { get; set; } = false;
        protected TypeNameContext Context { get; set; }
        protected Recorder Recorder { get; set; }
        public ActionNameConvention NameConvention { get; set; }

        public BeforeMapPrinter(TypeNameContext context, Recorder recorder)
        {
            NameConvention = NameConventions.BeforeMap;

            Context = context;
            Recorder = recorder;

            var statements = Context.TypeMap.BeforeMapStatements;

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

                Recorder.AttachRawCode("{{ ");

                Recorder.AppendLine(text, text);
            }
        }

        private string ToCode(OriginalStatement condition)
        {
            string methodCall = ToTemplate(condition).TemplateToCode(Context.SrcMemberPrefix, Context.DestMemberPrefix);

            return methodCall;
        }

        private string ToTemplate(OriginalStatement condition)
        {
            string memberName = NameConvention.GetMemberFullName(condition.Id);
            string context = $"new {typeof(ResolutionContext).FullName}()";

            string call = $"{memberName}({{0}}, {{1}}, {context});";

            return call;
        }

        public void Dispose()
        {
            if (IsExist)
            {
                Recorder.AttachRawCode("}}");
            }
        }
    }
}