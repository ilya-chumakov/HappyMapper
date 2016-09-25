using System;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class Recorder
    {
        public Recorder()
        {
            CodeBuilder = new StringBuilder();
            TemplateBuilder = new StringBuilder();
        }

        public StringBuilder TemplateBuilder { get; set; }

        public StringBuilder CodeBuilder { get; set; }

        public void AppendLine(string code, string template)
        {
            CodeBuilder.AppendLine(code);
            TemplateBuilder.AppendLine(template);
        }

        /// <summary>
        /// destPrefix.Foo = srcPrefix.Foo;
        /// </summary>
        /// <param name="srcPrefix"></param>
        /// <param name="destPrefix"></param>
        /// <param name="srcMemberName"></param>
        /// <param name="destMemberName"></param>
        public void AssignAsNoCast(string srcPrefix, string destPrefix, string srcMemberName, string destMemberName)
        {
            string template = $"{{1}}.{destMemberName} = {{0}}.{srcMemberName};";

            string compiled = string.Format(template, srcPrefix, destPrefix);

            AppendLine(compiled, template);
        }

        /// <summary>
        /// destPrefix.Foo = srcPrefix.Foo;
        /// </summary>
        /// <param name="srcPrefix"></param>
        /// <param name="destPrefix"></param>
        /// <param name="srcMemberName"></param>
        /// <param name="destMemberName"></param>
        public void AssignAsExplicitCast(string srcPrefix, string destPrefix, string srcMemberName, string destMemberName, string typeToCast)
        {
            string template = $"{{1}}.{destMemberName} = ({typeToCast}) {{0}}.{srcMemberName};";

            string compiled = string.Format(template, srcPrefix, destPrefix);

            AppendLine(compiled, template);
        }

        public void AssignAsToStringCall(string srcPrefix, string destPrefix, string srcMemberName, string destMemberName)
        {
            string template = $"{{1}}.{destMemberName} = {{0}}.{srcMemberName}.ToString();";

            string compiled = string.Format(template, srcPrefix, destPrefix);

            AppendLine(compiled, template);
        }

        public void AssignAsNoCast(PropertyNameContext context)
        {
            AssignAsNoCast(context.SrcMemberPrefix, context.DestMemberPrefix, 
                context.SrcMemberName, context.DestMemberName);
        }

        public void AssignAsExplicitCast(PropertyNameContext context)
        {
            AssignAsExplicitCast(context.SrcMemberPrefix, context.DestMemberPrefix, 
                context.SrcMemberName, context.DestMemberName, context.DestTypeFullName);
        }

        public void AssignAsToStringCall(PropertyNameContext context)
        {
            AssignAsToStringCall(context.SrcMemberPrefix, context.DestMemberPrefix,
                context.SrcMemberName, context.DestMemberName);
        }

        public void ApplyTemplate(PropertyNameContext context, string text)
        {
            ApplyTemplate(context.SrcMemberName, context.DestMemberName, text);
        }

        public void ApplyTemplate(string src, string dest, string template)
        {
            string formattedText = string.Format(template, src, dest);
            AppendLine(formattedText, template);
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
            string shiftedTemplate = ShiftTemplate(
                assignment.RelativeTemplate, propertyMap.SrcMember.Name, propertyMap.DestMember.Name);

            AppendLine(assignment.Code, shiftedTemplate);
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
            AppendLine(text, text);
        }

        public void AttachRawCode(string raw)
        {
            AppendLine(raw, raw);
        }

        //TODO refactor
        public void AppendNoParameterlessCtorException(PropertyNameContext context, Type destPropType)
        {
            //has parameterless ctor
            if (destPropType.GetConstructor(Type.EmptyTypes) != null)
            {
                //create new Dest() object
                string fullName = destPropType.FullName.NormalizeTypeName();
                string template = $"{{0}}.{context.DestMemberName} = new {fullName}();";

                string code = string.Format(template, context.DestMemberPrefix);
                AppendLine(code, template);
            }
            else
            {
                string exMessage =
                    ErrorMessages.NoParameterlessCtor($"{context.SrcMemberName}", $"{context.DestMemberName}", destPropType);

                string template = $@"if ({{0}}.{context.DestMemberName} == null) throw new OrdinaryMapperException(""{exMessage}"");";
                string code = string.Format(template, context.DestMemberPrefix);

                AppendLine(code, template);
            }
        }
    }
}