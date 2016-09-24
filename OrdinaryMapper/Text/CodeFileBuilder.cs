using System;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class CodeFileBuilder
    {
        public TypePair TypePair { get; set; }
        public MapperNameConvention Convention { get; set; } = NameConventions.Mapper;

        public string DestFullName { get; }
        public string SrcFullName { get; }

        public CodeFileBuilder(TypePair typePair)
        {
            TypePair = typePair;
            SrcFullName = typePair.SourceType.FullName;
            DestFullName = typePair.DestinationType.FullName;
        }

        public CodeFile CreateCodeFile(string methodCode)
        {
            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";
            string methodName = "Map";
            string shortClassName = Convention.GetUniqueMapperMethodNameWithGuid(TypePair);
            string fullClassName = $"{Convention.Namespace}.{shortClassName}";


            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using OrdinaryMapper;                                                       ");

            builder.AppendLine($"namespace {Convention.Namespace}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"   public static class {shortClassName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public static void {methodName}");
            builder.AppendLine($"({SrcFullName.NormalizeTypeName()} {srcParameterName},");
            builder.AppendLine($" {DestFullName.NormalizeTypeName()} {destParameterName})");
            builder.AppendLine("        {");

            builder.AppendLine(methodCode);

            builder.AppendLine("        }");

            builder.AppendLine("    }                                                               ");
            builder.AppendLine("}");

            string code = builder.ToString();

            var file = new CodeFile(code, fullClassName, methodName, TypePair);

            return file;
        }
    }
}