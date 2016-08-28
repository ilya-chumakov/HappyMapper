using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrdinaryMapper
{
    public class MapperTextBuilderV2
    {
        public Dictionary<TypePair, string> TemplateCache { get; } = new Dictionary<TypePair, string>();

        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }
        public IDictionary<TypePair, TypeMap> ImplicitTypeMaps { get; }
        public TypeMapFactory TypeMapFactory { get; } = new TypeMapFactory();
        public MapperConfigurationExpression Options { get; }

        public MapperTextBuilderV2(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            ImplicitTypeMaps = explicitTypeMaps.ShallowCopy();
            Options = mce;
        }

        public string CreateText(TypeMap map)
        {
            string srcFieldName = "src";
            string destFieldName = "dest";

            var assignment = ProcessTypeMap(map, srcFieldName, destFieldName);

            return assignment.Code;
        }

        private Assignment ProcessTypeMap(TypeMap rootMap, string srcFieldName, string destFieldName)
        {
            var coder = new Coder();

            foreach (PropertyMap propertyMap in rootMap.PropertyMaps)
            {
                var context = new PropertyNameContext(propertyMap, srcFieldName, destFieldName);

                //TODO: generate new Dest() instance
                if (propertyMap.Ignored) continue;

                //assign without explicit cast
                if (propertyMap.DestType.IsAssignableFrom(propertyMap.SrcType)
                    || propertyMap.DestType.IsImplicitCastableFrom(propertyMap.SrcType))
                {
                    //TODO: need to determine explicit casts and produce cast operators
                    coder.SimpleAssign(context);
                    continue;
                }
                else
                {
                    bool referenceType = propertyMap.DestType.IsClass;

                    if (referenceType)
                    {
                        coder.NullCheck(context);
                        coder.AttachRawCode(" else {{");

                        coder.AppendNoParameterlessCtorException(context, propertyMap.DestType);
                    }

                    ProcessPropertyTypePair(coder, context, propertyMap);

                    if (referenceType) coder.AttachRawCode("}}");
                }
            }

            var assignment = coder.GetAssignment();

            TemplateCache.AddIfNotExist(rootMap.TypePair, assignment.RelativeTemplate);

            return assignment;
        }

        private void ProcessPropertyTypePair(Coder coder, PropertyNameContext context, PropertyMap propertyMap)
        {
            var typePair = propertyMap.GetTypePair();

            //typepair already in template cache
            string text;
            if (TemplateCache.TryGetValue(typePair, out text))
            {
                coder.ApplyTemplate(context, text);
                return;
            }
            
            TypeMap nodeMap;

            //typepair explicitly mapped by user
            ExplicitTypeMaps.TryGetValue(typePair, out nodeMap);

            if (nodeMap == null)
            {
                if (!ImplicitTypeMaps.TryGetValue(typePair, out nodeMap))
                {
                    //create implicit map 
                    nodeMap = TypeMapFactory.CreateTypeMap(propertyMap.SrcType, propertyMap.DestType, Options);
                    ImplicitTypeMaps.AddIfNotExist(nodeMap);
                }
            }

            var propAssignment = ProcessTypeMap(nodeMap, context.SrcFullMemberName, context.DestFullMemberName);
            coder.AttachPropertyAssignment(propAssignment, propertyMap);
            return;
        }

        private string CreatePropertiesAssignments(
            PropertyInfo[] srcProperties,
            PropertyInfo[] destProperties,
            string srcPrefix,
            string destPrefix)
        {
            var coder = new Coder();
            var builder = new StringBuilder();
            foreach (var srcProperty in srcProperties)
            {
                string name = srcProperty.Name;

                var destProperty = destProperties.First(p => p.Name == name);

                Type srcPropType = srcProperty.PropertyType;
                Type destPropType = destProperty.PropertyType;

                if (destPropType.IsAssignableFrom(srcPropType))
                {
                    coder.SimpleAssign(srcPrefix, destPrefix, name, name);
                    //builder.AppendLine($"{destPrefix}.{name} = {srcPrefix}.{name};");
                }
                else
                {
                    if (srcPropType.IsClass && destPropType.IsClass)
                    {
                        builder.AppendLine($"if ({srcPrefix}.{name} == null) {destPrefix}.{name} = null;");
                        builder.AppendLine("else");
                        builder.AppendLine("{");

                        //coder.AppendNoParameterlessCtorException(context, destPropType);

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

        public string CreateText(MapContext context)
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
            builder.AppendLine($"    public class {MapContext.MapperClassName}                  ");
            builder.AppendLine("    {                                                               ");
            builder.AppendLine($"       public void {context.MapperMethodName}");
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

    }

}