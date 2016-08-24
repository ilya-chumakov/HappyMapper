using System;
using System.Linq;
using System.Reflection;
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

            string srcParameterName = "src";
            string destParameterName = "dest";

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine($"namespace {MapContext.NamespaceName}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"    public static class {MapContext.MapperClassName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public static void {context.MapperMethodName}");
            builder.AppendLine($"({context.SrcType.FullName} {srcParameterName},");
            builder.AppendLine($" {context.DestType.FullName} {destParameterName})");
            builder.AppendLine("        {");

            string assignments = CreatePropertiesAssignments(srcProperties, destProperties, srcParameterName, destParameterName);
            builder.AppendLine(assignments);

            builder.AppendLine("        }");

            builder.AppendLine("    }                                                               ");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static string CreatePropertiesAssignments(
            PropertyInfo[] srcProperties,
            PropertyInfo[] destProperties,
            string srcPrefix,
            string destPrefix)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var srcProperty in srcProperties)
            {
                string name = srcProperty.Name;

                var destProperty = destProperties.First(p => p.Name == name);

                Type srcPropType = srcProperty.PropertyType;
                Type destPropType = destProperty.PropertyType;

                if (destPropType.IsAssignableFrom(srcPropType))
                {
                    builder.AppendLine($"{destPrefix}.{name} = {srcPrefix}.{name};");
                }
                else
                {
                    if (srcPropType.IsClass && destPropType.IsClass)
                    {
                        builder.AppendLine($"if ({srcPrefix}.{name} == null) {destPrefix}.{name} = null;");
                        builder.AppendLine("else");
                        builder.AppendLine("{");

                        //has parameterless ctor
                        if (destPropType.GetConstructor(Type.EmptyTypes) != null)
                            //create new Dest() object
                            builder.AppendLine($"{destPrefix}.{name} = new {destPropType.FullName}();");
                        else
                            builder.AppendLine($"if ({destPrefix}.{name} == null) throw new NullReferenceException();");

                        string text = CreatePropertiesAssignments(
                            srcPropType.GetProperties(), 
                            destPropType.GetProperties(),
                            $"{srcPrefix}.{name}",
                            $"{destPrefix}.{name}");

                        builder.AppendLine(text);

                        builder.AppendLine("}");
                    }
                }
            }

            return builder.ToString();
        }
    }
}