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

        public CodeFile CreateCodeFile(string methodInnerCode)
        {
            string methodName = "Map";
            string shortClassName = Convention.GetUniqueMapperMethodNameWithGuid(TypePair);
            string fullClassName = $"{Convention.Namespace}.{shortClassName}";

            string methodCode = WrapMethodCode(methodInnerCode, methodName);

            string classCode = CodeHelper.BuildClassCode(methodCode, Convention.Namespace, shortClassName);

            return new CodeFile(classCode, fullClassName, methodName, TypePair);
        }

        public string WrapMethodCode(string inner, string methodName)
        {
            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            builder.AppendLine($"       public static void {methodName}");
            builder.AppendLine($"({SrcFullName.NormalizeTypeName()} {srcParameterName},");
            builder.AppendLine($" {DestFullName.NormalizeTypeName()} {destParameterName})");
            builder.AppendLine("        {");

            builder.AppendLine(inner);

            builder.AppendLine("        }");

            return builder.ToString();
        }
    }
}