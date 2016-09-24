using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrdinaryMapper
{
    public static class MapperTextBuilder
    {
        public static MapperNameConvention Convention { get; set; }

        public static string CreateText(MapContext context)
        {
            Convention = NameConventions.Mapper;

            var srcProperties = context.SrcType.GetProperties();
            var destProperties = context.DestType.GetProperties();

            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            string methodName = Convention.GetMapperMethodName(context.SrcType, context.DestType);

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using OrdinaryMapper;                                                       ");

            builder.AppendLine($"namespace {Convention.Namespace}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"    public static class {Convention.ClassShortName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public static void {methodName}");
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
                        {
                            string exMessage =
                                ErrorMessages.NoParameterlessCtor($"{name}", $"{name}", destPropType);

                            builder.AppendLine($@"if ({destPrefix}.{name} == null) throw new OrdinaryMapperException(""{exMessage}"");");
                        }

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