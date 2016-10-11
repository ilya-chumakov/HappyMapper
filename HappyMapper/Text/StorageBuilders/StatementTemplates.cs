using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Extended.Net4;

namespace HappyMapper.Text
{
    public static class StatementTemplates
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
            builder.AppendLine("using System.Collections.Generic; "); //TODO: only for CollectionFileBuilder
            builder.AppendLine("using System.Linq; "); //TODO: only for CollectionFileBuilder
            builder.AppendLine($"using {NameConventionsStorage.Mapper.Namespace}; ");

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

            string returnType = ctx.Return?.Type ?? "void";
            string returnStatement = ctx.Return != null ?
                $"        return {ctx.Return.Name};"
                : string.Empty;

            builder.AppendLine($"       public static {returnType} ");
            builder.AppendLine($"       {ctx.MethodName}");
            builder.AppendLine("        (");

            string arguments = string.Join(
                "," + Environment.NewLine, 
                ctx.Arguments.Select(arg => $"{arg.Type} {arg.Name}"));

            builder.AppendLine("        " + arguments);
            builder.AppendLine("        )");
            builder.AppendLine("        {");
            builder.AppendLine(innerCode + ";");

            builder.AppendLine(returnStatement + ";");

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

        /// <summary>
        /// if ({src} == null)
        /// TODO: perfomance degrades on each null check! Try to avoid it if possible!
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string IfNull(string src) => $"if ({src} == null)";

        public static string New(string type) => $"new {type}()";

        public static string MethodCall(string methodName, params string[] arguments)
        {
            string joinedArguments = string.Join("," + Environment.NewLine, arguments);

            return $"{methodName}(\n{joinedArguments}\n)";
        }
    }
}