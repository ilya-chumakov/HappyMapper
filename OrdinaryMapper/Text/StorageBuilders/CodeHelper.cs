using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrdinaryMapper
{
    public static class CodeHelper
    {
        public static string BuildClassCode(List<string> methods, string nms, string className)
        {
            if (methods == null || !methods.Any()) return string.Empty;

            var builder = new StringBuilder();

            builder.AppendLine("using System; ");
            builder.AppendLine($"using {NameConventions.Mapper.Namespace}; ");

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