using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrdinaryMapper
{
    public static class MapperTextBuilderV2
    {
        public static Dictionary<TypePair, string> TemplateCache { get; } = new Dictionary<TypePair, string>();

        public static string CreateText(TypeMap map, IDictionary<TypePair, TypeMap> typeMaps)
        {
            string srcFieldName = "src";
            string destFieldName = "dest";

            var assignment = ProcessTypeMap(map, typeMaps, srcFieldName, destFieldName);

            return assignment.Code;
        }

        private static TypeAssignment ProcessTypeMap(TypeMap rootMap, IDictionary<TypePair, TypeMap> allTypeMaps, string srcFieldName,
            string destFieldName)
        {
            var coder = new Coder();

            foreach (PropertyMap propertyMap in rootMap.PropertyMaps)
            {
                if (propertyMap.DestinationPropertyType.IsAssignableFrom(propertyMap.SourceType)
                    || propertyMap.DestinationPropertyType.IsImplicitCastableFrom(propertyMap.SourceType))
                {
                    //TODO: need to determine explicit casts and produce cast operators
                    coder.SimpleAssign(srcFieldName, destFieldName, propertyMap);
                    continue;
                }

                var propertyTypePair = propertyMap.GetTypePair();

                string text;
                if (TemplateCache.TryGetValue(propertyTypePair, out text))
                {
                    coder.AttachTemplate(srcFieldName, destFieldName, propertyMap, text);
                    continue;
                }

                TypeMap nodeMap;
                if (allTypeMaps.TryGetValue(propertyTypePair, out nodeMap))
                {
                    string srcPrefix = Combine(srcFieldName, propertyMap.SourceMember.Name);
                    string destPrefix = Combine(destFieldName, propertyMap.DestinationProperty.Name);

                    var propAssignment = ProcessTypeMap(nodeMap, allTypeMaps, srcPrefix, destPrefix);

                    coder.AttachAssignment(propAssignment);
                    continue;
                }

                //TODO scan via Reflection
            }

            var assignment = coder.GetAssignment();

            TemplateCache.AddIfNotExist(rootMap.TypePair, assignment.Template);

            return assignment;
        }

        private static string Combine(string left, string right)
        {
            return $"{left}.{right}";
        }


        public static string CreateText(MapContext context)
        {


            var srcProperties = context.SrcType.GetProperties();
            var destProperties = context.DestType.GetProperties();

            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using OrdinaryMapper;                                                       ");

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