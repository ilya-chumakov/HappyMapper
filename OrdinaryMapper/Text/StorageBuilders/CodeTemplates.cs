using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    public static class CodeTemplates
    {
        public static string Class(string method, string nms, string className)
        {
            return Class(new List<string>(new[] { method }), nms, className);
        }

        public static string Class(List<string> methods, string nms, string className)
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

        public static string Method(string innerCode, MethodDeclarationContext ctx)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"       public static void {ctx.MethodName}");
            builder.AppendLine($"({ctx.SrcType} {ctx.SrcParam},");
            builder.AppendLine($" {ctx.DestType} {ctx.DestParam})");

            builder.AppendLine("        {");
            builder.AppendLine(innerCode);
            builder.AppendLine("        }");

            return builder.ToString();
        }

        public static string For(string innerCode, ForDeclarationContext ctx)
        {
            var builder = new StringBuilder();

            string index = "index_" + NamingTools.NewGuid(4);

            //builder.AppendLine($@"if ({ctx.SrcCollection}.Count != {ctx.DestCollection}.Count)");
            //builder.AppendLine($@"throw new NotImplementedException(""{ctx.SrcCollection}.Count != {ctx.DestCollection}.Count"");");

            builder.AppendLine($"           for (int {index} = 0; {index} < {ctx.SrcCollection}.Count; {index}++)");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {ctx.SrcVariable} = {ctx.SrcCollection}.ElementAt({index});");
            builder.AppendLine($"                var {ctx.DestVariable} = {ctx.DestCollection}.ElementAt({index});");
            builder.AppendLine(innerCode);
            builder.AppendLine("            }");

            string forCode = builder.ToString();
            return forCode;
        }


        public static string NullCheck(string src, string dest) => $"if ({src} == null) {dest} = null;";
    }
}