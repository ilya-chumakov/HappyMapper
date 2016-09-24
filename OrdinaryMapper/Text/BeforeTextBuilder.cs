using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using AutoMapper.ConfigurationAPI;

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

                foreach (var action in map.BeforeMapActions)
                {
                    if (action != null)
                    {
                        string methodCode = CreateMethodInnerCode(action);

                        methodCode = methodCode.Replace("{{", "").Replace("}}", "");

                        methods.Add(methodCode);
                    }
                }
            }

            var file = CreateCodeFile(methods, "OrdinaryMapper", "BeforeActionStore");

            return file;
        }

        private string CreateMethodInnerCode(LambdaExpression action)
        {
            //string id = action.OriginalCondition.Id;
            //string srcTypeName = action.TypeMap.SourceType.FullName.NormalizeTypeName();
            //string destTypeName = action.TypeMap.DestinationType.FullName.NormalizeTypeName();
            //string type = $"Func<{srcTypeName}, {destTypeName}, bool>";

            var builder = new StringBuilder();

            //builder.AppendLine($"public static {type} Condition_{id};                               ");

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