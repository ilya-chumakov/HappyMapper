using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using Microsoft.CodeAnalysis.CSharp;

namespace OrdinaryMapper
{
    public class Compiler
    {
        public static Dictionary<TypePair, object> CompileMapsToAssembly(MapperConfigurationExpression config, IDictionary<TypePair, TypeMap> typeMaps)
        {
            var textBuilder = new MapperTextBuilderV2(typeMaps, config);
            var files = textBuilder.CreateCodeFiles();
            string[] trees = files.Values.Select(x => x.Code).ToArray();

            var cb = new ConditionTextBuilder(typeMaps);
            var f2 = cb.CreateCodeFile();

            trees = trees.Union(new[] { f2.Code }).ToArray();

            HashSet<string> locations = textBuilder.DetectedLocations;

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(trees, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitConditionStore(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, files, assembly);

            return delegateCache;
        }

        private static void InitConditionStore(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var type = assembly.GetType("OrdinaryMapper.ConditionStore");

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (PropertyMap propertyMap in map.PropertyMaps)
                {
                    if (propertyMap.OriginalCondition != null)
                    {
                        string id = propertyMap.OriginalCondition.Id;
                        var func = propertyMap.OriginalCondition.Fnc;

                        var fieldInfo = type.GetField($"Condition_{id}");

                        fieldInfo.SetValue(null, func);
                    }
                }
            }
        }

        private static Dictionary<TypePair, object> CreateDelegateCache(IDictionary<TypePair, TypeMap> typeMaps, Dictionary<TypePair, CodeFile> files, Assembly assembly)
        {
            Dictionary<TypePair, object> delegateCache = new Dictionary<TypePair, object>();

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;
                CodeFile codeFile = files[typePair];

                var type = assembly.GetType(codeFile.ClassFullName);

                var @delegate = Delegate.CreateDelegate(map.MapDelegateType, type, codeFile.MapperMethodName);

                delegateCache.Add(typePair, @delegate);
            }
            return delegateCache;
        }
    }

    public class ConditionTextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }

        public ConditionTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
        }


        public CodeFile CreateCodeFile()
        {
            var files = new Dictionary<TypePair, CodeFile>();

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