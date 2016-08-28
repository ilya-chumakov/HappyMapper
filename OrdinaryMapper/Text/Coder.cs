using System;
using System.Text;

namespace OrdinaryMapper
{
    public class Coder
    {
        public Coder()
        {
            CodeBuilder = new StringBuilder();
            TemplateBuilder = new StringBuilder();
        }

        public StringBuilder TemplateBuilder { get; set; }

        public StringBuilder CodeBuilder { get; set; }

        /// <summary>
        /// destPrefix.Foo = srcPrefix.Foo;
        /// </summary>
        /// <param name="srcPrefix"></param>
        /// <param name="destPrefix"></param>
        /// <param name="srcMemberName"></param>
        /// <param name="destMemberName"></param>
        public void SimpleAssign(string srcPrefix, string destPrefix, string srcMemberName, string destMemberName)
        {
            string template = $"{{1}}.{srcMemberName} = {{0}}.{destMemberName};";

            string compiled = string.Format(template, srcPrefix, destPrefix);

            TemplateBuilder.AppendLine(template);
            CodeBuilder.AppendLine(compiled);
        }

        internal void SimpleAssign(PropertyNameContext context)
        {
            SimpleAssign(context.SrcMemberPrefix, context.DestMemberPrefix, context.SrcMemberName, context.DestMemberName);
        }

        internal void ApplyTemplate(PropertyNameContext context, string text)
        {
            ApplyTemplate(context.SrcMemberName, context.DestMemberName, text);
        }

        public void ApplyTemplate(string src, string dest, string text)
        {
            TemplateBuilder.AppendLine(text);

            string formattedText = string.Format(text, src, dest);
            CodeBuilder.AppendLine(formattedText);
        }

        public Assignment GetAssignment()
        {
            var assignment = new Assignment();
            assignment.Code = CodeBuilder.ToString();
            assignment.RelativeTemplate = TemplateBuilder.ToString();
            return assignment;
        }

        public void AttachPropertyAssignment(Assignment assignment, PropertyMap propertyMap)
        {
            CodeBuilder.Append(assignment.Code);

            string shiftedTemplate = ShiftTemplate(
                assignment.RelativeTemplate, propertyMap.SrcMember.Name, propertyMap.DestMember.Name);

            TemplateBuilder.Append(shiftedTemplate);
        }

        public static string ShiftTemplate(string template, string srcName, string destName)
        {
            return string.Format(template, 
                "{0}." + srcName,
                "{1}." + destName);
        }

        public void NullCheck(PropertyNameContext context)
        {
            string text = $"if ({context.SrcFullMemberName} == null) {context.DestFullMemberName} = null;";
            CodeBuilder.AppendLine(text);
            TemplateBuilder.AppendLine(text);
        }

        public void AttachRawCode(string raw)
        {
            CodeBuilder.AppendLine(raw);
            TemplateBuilder.AppendLine(raw);
        }
    }
}