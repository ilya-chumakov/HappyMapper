using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            var bmb = new BeforeTextBuilder(typeMaps);
            var f3 = bmb.CreateCodeFile();

            trees = trees.Union(new[] { f2.Code, f3.Code }).ToArray();

            PrintTrees(trees);

            HashSet<string> locations = textBuilder.DetectedLocations;

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(trees, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitConditionStore(typeMaps, assembly);
            InitBeforeActionStore(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, files, assembly);

            return delegateCache;
        }

        private static void PrintTrees(string[] trees)
        {
            foreach (string tree in trees)
            {
                //Console.WriteLine(tree);
                Debug.WriteLine(tree);
                //Trace.WriteLine(tree);
            }
        }

        private static void InitBeforeActionStore(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var conv = NameConventionConfig.BeforeMapActionNameConvention;

            var type = assembly.GetType(conv.ClassFullName);

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (var action in map.BeforeMapStatements)
                {
                    if (action != null)
                    {
                        var fieldInfo = type.GetField(conv.GetMemberShortName(action.Id));

                        fieldInfo.SetValue(null, action.Delegate);
                    }
                }
            }
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
                        var func = propertyMap.OriginalCondition.Delegate;

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
}