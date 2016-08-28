using System;
using System.Text;

namespace OrdinaryMapper
{
    public class CodeFileBuilder
    {
        public TypePair TypePair { get; set; }

        public const string NamespaceName = "RoslynMappers";

        public string MapperClassName { get; private set; }
        public string MapperMethodName { get; }
        public string MapperClassFullName { get; }
        public string DestFullName { get; }
        public string SrcFullName { get; }

        public CodeFileBuilder(TypePair typePair)
        {
            TypePair = typePair;
            SrcFullName = typePair.SrcType.FullName;
            DestFullName = typePair.DestType.FullName;

            CreateClassName(typePair);
            MapperClassFullName = $"{NamespaceName}.{MapperClassName}";
            MapperMethodName = "Map";
            //MapperMethodName = $"{NamingTools.ToAlphanumericOnly(SrcFullName)}_{NamingTools.ToAlphanumericOnly(DestFullName)}";
        }

        private void CreateClassName(TypePair typePair)
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");

            string normSrcName = NamingTools.ToAlphanumericOnly(typePair.SrcType.Name);
            string normDestName = NamingTools.ToAlphanumericOnly(typePair.DestType.Name);

            MapperClassName = $"Mapper_{normSrcName}_{normDestName}_{guid}";
        }


        public CodeFile CreateCodeFile(string methodCode)
        {
            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using OrdinaryMapper;                                                       ");

            builder.AppendLine($"namespace {NamespaceName}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"   public static class {MapperClassName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public static void {MapperMethodName}");
            builder.AppendLine($"({SrcFullName.NormalizeTypeName()} {srcParameterName},");
            builder.AppendLine($" {DestFullName.NormalizeTypeName()} {destParameterName})");
            builder.AppendLine("        {");

            builder.AppendLine(methodCode);

            builder.AppendLine("        }");

            builder.AppendLine("    }                                                               ");
            builder.AppendLine("}");

            string code = builder.ToString();

            var file = new CodeFile(code, MapperClassFullName, MapperMethodName, TypePair);

            return file;
        }
    }

    public class CodeFile
    {
        public string Code { get; }
        public TypePair TypePair { get; }
        public string ClassFullName { get; }
        public string MapperMethodName { get; }

        public CodeFile(string code, string classFullName, string mapperMethodName, TypePair typePair)
        {
            Code = code;
            ClassFullName = classFullName;
            MapperMethodName = mapperMethodName;
            TypePair = typePair;
        }
   }
}