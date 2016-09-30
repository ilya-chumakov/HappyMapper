using System;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public enum Assign
    {
        AsNoCast,
        AsExplicitCast,
        AsToStringCall,
        AsStringToValueTypeConvert
    }

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
        public string GetAssignTemplate(Assign assignType, IAssignContext context)
        {
            switch (assignType)
            {
                case OrdinaryMapper.Assign.AsNoCast:
                    return $"{{1}}.{context.DestMemberName} = {{0}}.{context.SrcMemberName};";

                case OrdinaryMapper.Assign.AsExplicitCast:
                    return $"{{1}}.{context.DestMemberName} = ({context.DestTypeFullName}) {{0}}.{context.SrcMemberName};";

                case OrdinaryMapper.Assign.AsToStringCall:
                    return $"{{1}}.{context.DestMemberName} = {{0}}.{context.SrcMemberName}.ToString();";

                case OrdinaryMapper.Assign.AsStringToValueTypeConvert:
                    return $"{{1}}.{context.DestMemberName} = ({context.DestTypeFullName}) Convert.ChangeType({{0}}.{context.SrcMemberName}, typeof({context.DestTypeFullName}));";

                default: throw new NotSupportedException(assignType.ToString());
            }
        }

        public void AppendAssignment(Assign assignType, IAssignContext context)
        {
            string template = GetAssignTemplate(assignType, context);

            ApplyTemplate(template, context.SrcMemberPrefix, context.DestMemberPrefix);
        }

        public void ApplyTemplate(string template, string src, string dest)
        {
            string formattedText = string.Format(template, src, dest);
            AppendLine(formattedText, template);
        }

        public Assignment ToAssignment()
        {
            var assignment = new Assignment();
            assignment.Code = CodeBuilder.ToString();
            assignment.RelativeTemplate = TemplateBuilder.ToString();
            return assignment;
        }

        /// <summary>
        /// Shift template and append.
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="propertyMap"></param>
        public void AppendPropertyAssignment(Assignment assignment, PropertyMap propertyMap)
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

        public void AppendRawCode(string raw)
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
                string template = $"{{1}}.{context.DestMemberName} = new {fullName}();";

                //string code = string.Format(template, context.DestMemberPrefix);
                string code = template.TemplateToCode("", context.DestMemberPrefix);
                AppendLine(code, template);
            }
            else
            {
                string exMessage =
                    ErrorMessages.NoParameterlessCtor($"{context.SrcMemberName}", $"{context.DestMemberName}", destPropType);

                string template = $@"if ({{1}}.{context.DestMemberName} == null) throw new OrdinaryMapperException(""{exMessage}"");";
                string code = template.TemplateToCode("", context.DestMemberPrefix);

                AppendLine(code, template);
            }
        }
    }
}