using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper.Text
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
            NameConvention = NameConventionsStorage.BeforeMap;

            Context = context;
            Recorder = recorder;

            var statements = Context.TypeMap.BeforeMapStatements;

            IsExist = statements.Any();

            if (IsExist)
            {
                List<string> templates = statements.Select(ToTemplate).ToList();

                string template = string.Join("", templates);

                Recorder.AppendLine("{{ ");

                Recorder.AppendLine(template);
            }
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
                Recorder.AppendLine("}}");
            }
        }
    }
}