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
            builder.AppendLine("using System.Collections.Generic; "); //TODO: only for CollectionTextBuilder
            builder.AppendLine("using System.Linq; "); //TODO: only for CollectionTextBuilder
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

        public static string WrapMethodCode(string methodInnerCode, MethodDeclarationContext ctx)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"       public static void {ctx.MethodName}");
            builder.AppendLine($"({ctx.SrcType} {ctx.SrcParam},");
            builder.AppendLine($" {ctx.DestType} {ctx.DestParam})");

            builder.AppendLine("        {");
            builder.AppendLine(methodInnerCode);
            builder.AppendLine("        }");

            return builder.ToString();
        }

        public static string WrapForCode(string methodInnerCode, ForDeclarationContext ctx)
        {
            var builder = new StringBuilder();

            builder.AppendLine($@"if ({ctx.SrcCollection}.Count != {ctx.DestCollection}.Count)");
            builder.AppendLine($@"throw new NotImplementedException(""{ctx.SrcCollection}.Count != {ctx.DestCollection}.Count"");");

            builder.AppendLine($"           for (int i = 0; i < {ctx.SrcCollection}.Count; i++)");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {ctx.SrcVariable} = {ctx.SrcCollection}.ElementAt(i);");
            builder.AppendLine($"                var {ctx.DestVariable} = {ctx.DestCollection}.ElementAt(i);");
            builder.AppendLine(methodInnerCode);
            builder.AppendLine("            }");

            string forCode = builder.ToString();
            return forCode;
        }
    }

    public class ForDeclarationContext
    {
        public string SrcVariable { get; set; }
        public string SrcCollection { get; set; }
        public string DestVariable { get; set; }
        public string DestCollection { get; set; }

        public ForDeclarationContext(string srcCollection, string destCollection, string srcVariable, string destVariable)
        {
            SrcVariable = srcVariable;
            SrcCollection = srcCollection;
            DestVariable = destVariable;
            DestCollection = destCollection;
        }
    }
}