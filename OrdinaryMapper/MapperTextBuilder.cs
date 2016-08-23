using System.Linq;
using System.Text;

namespace OrdinaryMapper
{
    public static class MapperTextBuilder
    {
        public static string CreateText(MapContext context)
        {
            var srcProperties = context.SrcType.GetProperties();
            var destProperties = context.DestType.GetProperties();

            var builder = new StringBuilder();

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine($"namespace {MapContext.NamespaceName}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"    public static class {MapContext.MapperClassName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public static void {context.MapperMethodName}({context.SrcType.FullName} src, {context.DestType.FullName} dest)");
            builder.AppendLine("        {");

            foreach (var srcProperty in srcProperties)
            {
                string name = srcProperty.Name;

                var destProperty = destProperties.First(p => p.Name == name);

                if (destProperty.PropertyType.IsAssignableFrom(srcProperty.PropertyType))
                {
                    builder.AppendLine($"dest.{name} = src.{name};");
                }
                else
                {
                    //custom map
                }
            }

            builder.AppendLine("        }");

            builder.AppendLine("    }                                                               ");
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}