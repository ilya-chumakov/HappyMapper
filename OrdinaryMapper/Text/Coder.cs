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

        public Assignment GetAssignment()
        {
            var assignment = new Assignment();
            assignment.Code = code.ToString();
            assignment.RelativeTemplate = templates.ToString();
            return assignment;
        }

        public void AttachPropertyAssignment(Assignment assignment, PropertyMap propertyMap)
        {
            code.Append(assignment.Code);

            string shiftedTemplate = ShiftTemplate(
                assignment.RelativeTemplate, propertyMap.SrcMember.Name, propertyMap.DestMember.Name);

            templates.Append(shiftedTemplate);
        }

        public static string ShiftTemplate(string template, string srcName, string destName)
        {
            return string.Format(template, 
                "{0}." + srcName,
                "{1}." + destName);
        }
    }
}