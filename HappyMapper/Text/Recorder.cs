using System;
using System.Text;

namespace HappyMapper.Text
{
    public enum Assign
    {
        AsNoCast,
        AsExplicitCast,
        AsToStringCall,
        AsStringToValueTypeConvert
    }

    internal class Recorder
    {
        public Recorder()
        {
            TemplateBuilder = new StringBuilder();
        }

        public StringBuilder TemplateBuilder { get; set; }

        public void Append(string template)
        {
            TemplateBuilder.Append(template);
        }

        public void AppendLine(string template)
        {
            TemplateBuilder.AppendLine(template);
        }

        public string GetAssignTemplate(Assign assignType, IAssignContext context)
        {
            switch (assignType)
            {
                case Assign.AsNoCast:
                    return $"{{1}}.{context.DestMemberName} = {{0}}.{context.SrcMemberName};";

                case Assign.AsExplicitCast:
                    return $"{{1}}.{context.DestMemberName} = ({context.DestTypeFullName}) {{0}}.{context.SrcMemberName};";

                case Assign.AsToStringCall:
                    return $"{{1}}.{context.DestMemberName} = {{0}}.{context.SrcMemberName}.ToString();";

                case Assign.AsStringToValueTypeConvert:
                    return $"{{1}}.{context.DestMemberName} = ({context.DestTypeFullName}) Convert.ChangeType({{0}}.{context.SrcMemberName}, typeof({context.DestTypeFullName}));";

                default: throw new NotSupportedException(assignType.ToString());
            }
        }

        public void AppendAssignment(Assign assignType, IAssignContext context)
        {
            string template = GetAssignTemplate(assignType, context);

            AppendLine(template);
        }

        public Assignment ToAssignment()
        {
            var assignment = new Assignment();
            assignment.RelativeTemplate = TemplateBuilder.ToString();
            return assignment;
        }
    }
}