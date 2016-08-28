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
                //TODO: generate new Dest() instance
                if (propertyMap.Ignored) continue;

                //assign without explicit cast
                if (propertyMap.DestType.IsAssignableFrom(propertyMap.SrcType)
                    || propertyMap.DestType.IsImplicitCastableFrom(propertyMap.SrcType))
                {
                    //TODO: need to determine explicit casts and produce cast operators
                    coder.SimpleAssign(srcFieldName, destFieldName, propertyMap.SrcMember.Name, propertyMap.DestMember.Name);
                    continue;
                }

                var propertyTypePair = propertyMap.GetTypePair();

                //typepair already in template cache
                string text;
                if (TemplateCache.TryGetValue(propertyTypePair, out text))
                {
                    coder.AttachTemplate(srcFieldName, destFieldName, propertyMap, text);
                    continue;
                }

                string srcPrefix = Combine(srcFieldName, propertyMap.SrcMember.Name);
                string destPrefix = Combine(destFieldName, propertyMap.DestMember.Name);

                //typepair explicitly mapped by user
                TypeMap nodeMap;
                if (ExplicitTypeMaps.TryGetValue(propertyTypePair, out nodeMap))
                {
                    var propAssignment = ProcessTypeMap(nodeMap, srcPrefix, destPrefix);

                    coder.AttachPropertyAssignment(propAssignment, propertyMap);
                    continue;
                }

                //create implicit map 
                {
                    TypeMap implicitTypeMap;
                    if (!ImplicitTypeMaps.TryGetValue(propertyTypePair, out implicitTypeMap))
                    {
                        implicitTypeMap = TypeMapFactory.CreateTypeMap(propertyMap.SrcType, propertyMap.DestType, Options);
                        ImplicitTypeMaps.AddIfNotExist(implicitTypeMap);
                    }

                    var propAssignment = ProcessTypeMap(implicitTypeMap, srcPrefix, destPrefix);
                    coder.AttachPropertyAssignment(propAssignment, propertyMap);
                    continue;
                }
            }

            var assignment = coder.GetAssignment();

            TemplateCache.AddIfNotExist(rootMap.TypePair, assignment.RelativeTemplate);

            return assignment;
        }

        private string Combine(string left, string right)
        {
            return $"{left}.{right}";
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