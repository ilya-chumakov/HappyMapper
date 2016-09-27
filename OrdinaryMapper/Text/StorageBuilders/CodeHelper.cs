using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrdinaryMapper
{
    public static class CodeHelper
    {
        public static string BuildClassCode(string method, string nms, string className)
        {
            return BuildClassCode(new List<string>(new[] { method }), nms, className);
        }

        public static string BuildClassCode(List<string> methods, string nms, string className)
        {
            if (methods == null || !methods.Any()) return String.Empty;

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

        public static string WrapMethodCode(string inner, string methodName, string srcType, string destType)
        {
            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            builder.AppendLine($"       public static void {methodName}");
            builder.AppendLine($"({srcType.NormalizeTypeName()} {srcParameterName},");
            builder.AppendLine($" {destType.NormalizeTypeName()} {destParameterName})");
            builder.AppendLine("        {");

            builder.AppendLine(inner);

            builder.AppendLine("        }");

            return builder.ToString();
        }
    }
}