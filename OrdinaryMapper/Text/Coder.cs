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