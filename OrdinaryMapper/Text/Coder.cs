using System.Text;

namespace OrdinaryMapper
{
    public class Coder
    {
        public Coder()
        {
            code = new StringBuilder();
            templates = new StringBuilder();
        }

        public StringBuilder templates { get; set; }

        public StringBuilder code { get; set; }

        public void SimpleAssign(string src, string dest, PropertyMap propertyMap)
        {
            string template = $"{{1}}.{propertyMap.SourceMember.Name} = {{0}}.{propertyMap.DestinationProperty.Name};";

            string compiled = string.Format(template, src, dest);

            templates.AppendLine(template);
            code.AppendLine(compiled);
        }

        public void AttachTemplate(string src, string dest, PropertyMap propertyMap, string text)
        {
            templates.AppendLine(text);

            string formattedText = string.Format(text, src, dest);
            code.AppendLine(formattedText);
        }

        public TypeAssignment GetAssignment()
        {
            var assignment = new TypeAssignment();
            assignment.Code = code.ToString();
            assignment.Template = templates.ToString();
            return assignment;
        }

        public void AttachAssignment(TypeAssignment propAssignment)
        {
            code.Append(propAssignment.Code);
            templates.Append(propAssignment.Template);
        }
    }
}