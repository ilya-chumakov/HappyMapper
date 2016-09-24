using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    public class BeforeTextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }

        public BeforeTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
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

                foreach (var action in map.BeforeMapStatements)
                {
                    if (action != null)
                    {
                        string methodCode = CreateMethodInnerCode(action, map);

                        methodCode = methodCode.Replace("{{", "").Replace("}}", "");

                        methods.Add(methodCode);
                    }
                }
            }

            var conv = NameConventions.BeforeMap;

            var file = CreateCodeFile(methods, conv.Namespace, conv.ClassShortName);

            return file;
        }

        private string CreateMethodInnerCode(OriginalStatement statement, TypeMap map)
        {
            string id = statement.Id;
            string srcTypeName = map.SourceType.FullName.NormalizeTypeName();
            string destTypeName = map.DestinationType.FullName.NormalizeTypeName();
            string contextTypeName = typeof (ResolutionContext).FullName;

            string type = $"Action<{srcTypeName}, {destTypeName}, {contextTypeName}>";

            var builder = new StringBuilder();

            builder.AppendLine($"public static {type} BeforeMapAction_{id};                               ");

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