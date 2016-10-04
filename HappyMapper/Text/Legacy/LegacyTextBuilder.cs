using System;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace HappyMapper.Text.Legacy
{
    public static class LegacyTextBuilder
    {
        public class MapperNameConvention
        {
            public string Namespace { get; set; }
            public string ClassShortName { get; set; }

            public string ClassFullName => $"{Namespace}.{ClassShortName}";

            public string GetMapperMethodName(Type srcType, Type destType)
            {
                return $"{NamingTools.ToAlphanumericOnly(srcType.FullName)}_{NamingTools.ToAlphanumericOnly(destType.FullName)}";
            }

            public string GetMapperMethodName(TypeMap tm)
            {
                return GetMapperMethodName(tm.SourceType, tm.DestinationType);
            }
        }

        public static MapperNameConvention CreateConvention()
        {
            var convention = new MapperNameConvention();

            convention.Namespace = "LegacyTextBuilder";
            convention.ClassShortName = "Mapper";

            return convention;
        }

        public static MapperNameConvention Convention { get; set; }

        public static string CreateText(LegacyMapContext context)
        {
            Convention = CreateConvention();

            var srcProperties = context.SrcType.GetProperties();
            var destProperties = context.DestType.GetProperties();

            var builder = new StringBuilder();

            string srcParameterName = "src";
            string destParameterName = "dest";

            string methodName = Convention.GetMapperMethodName(context.SrcType, context.DestType);

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using HappyMapper;                                                       ");

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

                            builder.AppendLine($@"if ({destPrefix}.{name} == null) throw new HappyMapperException(""{exMessage}"");");
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