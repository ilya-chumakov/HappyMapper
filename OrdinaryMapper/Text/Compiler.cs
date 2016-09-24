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
        private  List<IStorageBuilder> StorageBuilders { get; set; } = new List<IStorageBuilder>();
        
         void RegisterStorageBuilders(IDictionary<TypePair, TypeMap> typeMaps)
        {
            StorageBuilders.Add(new BeforeStorageBuilder(typeMaps));
            StorageBuilders.Add(new ConditionStorageBuilder(typeMaps));
        }

        public  Dictionary<TypePair, object> CompileMapsToAssembly(
            MapperConfigurationExpression config, 
            IDictionary<TypePair, TypeMap> typeMaps)
        {
            RegisterStorageBuilders(typeMaps);

            var textBuilder = new MapperTextBuilderV2(typeMaps, config);
            var files = textBuilder.CreateCodeFiles();

            string[] sourceCodes = files.Values.Select(x => x.Code).ToArray();

            var storageCodes = BuildStorageCode();

            sourceCodes = sourceCodes.Union(storageCodes).ToArray();

            PrintSourceCode(sourceCodes);

            HashSet<string> locations = textBuilder.DetectedLocations;

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(sourceCodes, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitConditionStore(typeMaps, assembly);
            InitBeforeActionStore(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, files, assembly);

            return delegateCache;
        }

        private  string[] BuildStorageCode()
        {
            return StorageBuilders.Select(builder => builder.BuildCode())
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToArray();
        }

        private  void PrintSourceCode(string[] trees)
        {
            foreach (string tree in trees)
            {
                //Console.WriteLine(tree);
                Debug.WriteLine(tree);
                //Trace.WriteLine(tree);
            }
        }

        private  void InitBeforeActionStore(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var conv = NameConventions.BeforeMap;

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

        private  void InitConditionStore(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var conv = NameConventions.Condition;

            var type = assembly.GetType(conv.ClassFullName);

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

                        var fieldInfo = type.GetField(conv.GetMemberShortName(id));

                        fieldInfo.SetValue(null, func);
                    }
                }
            }
        }

        private  Dictionary<TypePair, object> CreateDelegateCache(
            IDictionary<TypePair, TypeMap> typeMaps, 
            Dictionary<TypePair, CodeFile> files, 
            Assembly assembly)
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