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
        private bool CreateCollectionMaps { get; } = false;
        
        void RegisterStorageBuilders(IDictionary<TypePair, TypeMap> typeMaps)
        {
            StorageBuilders.Add(new BeforeStorageBuilder(typeMaps));
            StorageBuilders.Add(new ConditionStorageBuilder(typeMaps));
        }

        public  Dictionary<TypePair, CompiledDelegate> CompileMapsToAssembly(
            MapperConfigurationExpression config, 
            IDictionary<TypePair, TypeMap> typeMaps)
        {
            RegisterStorageBuilders(typeMaps);

            var textBuilder = new MapperTextBuilderV2(typeMaps, config);
            var files = textBuilder.CreateCodeFiles();
            Dictionary<TypePair, CodeFile> collectionFiles = new Dictionary<TypePair, CodeFile>();

            if (CreateCollectionMaps)
            {
                var ctb = new CollectionTextBuilder(typeMaps, config);
                collectionFiles = ctb.CreateCodeFiles(files);
            }

            string[] sourceCodes = files.Values.Select(x => x.Code).ToArray();
            string[] collectionSourceCodes = collectionFiles.Values.Select(x => x.Code).ToArray();

            var storageCodes = BuildStorageCode();

            sourceCodes = sourceCodes
                .Union(storageCodes)
                .Union(collectionSourceCodes)
                .ToArray();

            PrintSourceCode(sourceCodes);

            HashSet<string> locations = textBuilder.MethodInnerCodeBuilder.DetectedLocations; //TODO: REFACTOR!!!!

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(sourceCodes, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitStorages(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, files, collectionFiles, assembly);

            return delegateCache;
        }

        private void InitStorages(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            StorageBuilders.ForEach(b => b.InitStorage(assembly));
        }

        private  string[] BuildStorageCode()
        {
            return StorageBuilders.Select(builder => builder.BuildCode())
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToArray();
        }

        private  void PrintSourceCode(string[] sourceCodes)
        {
            foreach (string code in sourceCodes)
            {
                Debug.WriteLine(code);
            }
        }

        private  Dictionary<TypePair, CompiledDelegate> CreateDelegateCache(
            IDictionary<TypePair, TypeMap> typeMaps, 
            Dictionary<TypePair, CodeFile> singleFiles, 
            Dictionary<TypePair, CodeFile> collectionFiles, 
            Assembly assembly)
        {
            var cache = new Dictionary<TypePair, CompiledDelegate>();

            foreach (var kvp in typeMaps)
            {
                var @delegate = new CompiledDelegate();

                TypeMap map = kvp.Value;
                TypePair typePair = kvp.Key;

                @delegate.Single = CreateDelegate(map.MapDelegateType, assembly, singleFiles[typePair]);

                if (CreateCollectionMaps)
                    @delegate.Collection = CreateDelegate(ToCollectionDelegateType(map), assembly, collectionFiles[typePair]);

                cache.Add(typePair, @delegate);
            }
            return cache;
        }

        private static Type ToCollectionDelegateType(TypeMap map)
        {
            var singleDelegateType = map.MapDelegateType;

            var srcType = singleDelegateType.GenericTypeArguments[0];
            var destType = singleDelegateType.GenericTypeArguments[1];

            var srcCollType = typeof(ICollection<>).MakeGenericType(srcType);
            var destCollType = typeof(ICollection<>).MakeGenericType(destType);

            return typeof(Action<,>).MakeGenericType(srcCollType, destCollType);
        }

        private static Delegate CreateDelegate(Type mapDelegateType, Assembly assembly, CodeFile codeFile)
        {
            var type = assembly.GetType(codeFile.ClassFullName);

            var @delegate = Delegate.CreateDelegate(mapDelegateType, type, codeFile.MapperMethodName);

            return @delegate;
        }
    }

    public class CompiledDelegate
    {
        public object Single { get; set; }
        public object Collection { get; set; }
    }
}