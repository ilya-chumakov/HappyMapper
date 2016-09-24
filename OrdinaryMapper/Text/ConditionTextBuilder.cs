using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class ConditionTextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }

        public ConditionTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
        }

        public CodeFile CreateCodeFile()
        {
            List<string> methods = new List<string>();

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (PropertyMap propertyMap in map.PropertyMaps)
                {
                    if (propertyMap.OriginalCondition != null)
                    {
                        string methodCode = CreateMethodInnerCode(propertyMap);

                        methodCode = methodCode.Replace("{{", "").Replace("}}", "");

                        methods.Add(methodCode);
                    }
                }
            }

            var file = CreateCodeFile(methods, "OrdinaryMapper", "ConditionStore");

            return file;
        }

        private string CreateMethodInnerCode(PropertyMap propertyMap)
        {
            string id = propertyMap.OriginalCondition.Id;
            string srcTypeName = propertyMap.TypeMap.SourceType.FullName.NormalizeTypeName();
            string destTypeName = propertyMap.TypeMap.DestinationType.FullName.NormalizeTypeName();
            string type = $"Func<{srcTypeName}, {destTypeName}, bool>";

            var builder = new StringBuilder();

            builder.AppendLine($"public static {type} Condition_{id};                               ");

            return builder.ToString();
        }

        public CodeFile CreateCodeFile(List<string> methods, string NamespaceName, string MapperClassName)
        {
            var builder = new StringBuilder();

            builder.AppendLine("using System;                                                       ");
            builder.AppendLine("using OrdinaryMapper;                                                       ");

            builder.AppendLine($"namespace {NamespaceName}                                ");
            builder.AppendLine("{                                                                   ");
            builder.AppendLine($"   public static class {MapperClassName}                  ");
            builder.AppendLine("    {                                                               ");

            foreach (string method in methods)
            {
                builder.AppendLine(method);
            }

            builder.AppendLine("    }                                                               ");
            builder.AppendLine("}");

            string code = builder.ToString();

            var file = new CodeFile(code, null, null, new TypePair());

            return file;
        }
    }
}