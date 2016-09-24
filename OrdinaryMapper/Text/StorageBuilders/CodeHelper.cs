using System.Collections.Generic;
using System.Text;

namespace OrdinaryMapper
{
    public static class CodeHelper
    {
        public static string BuildClassCode(List<string> methods, string nms, string className)
        {
            var builder = new StringBuilder();

            builder.AppendLine("using System; ");
            builder.AppendLine("using OrdinaryMapper; ");

            builder.AppendLine($"namespace {nms}");
            builder.AppendLine("{ ");
            builder.AppendLine($"   public static class {className}");
            builder.AppendLine("    { ");

            foreach (string method in methods)
            {
                builder.AppendLine(method);
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            string code = builder.ToString();

            return code;
        }
    }
}