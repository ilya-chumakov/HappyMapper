using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace HappyMapper.Compilation
{


    public class Compiler
    {
        private TextBuilderRegistry TextBuilderRegistry { get; set; } = new TextBuilderRegistry();
        
        private  List<IStorageBuilder> StorageBuilders { get; set; } = new List<IStorageBuilder>();
        private bool CreateCollectionMaps { get; } = true;
        

        void RegisterStorageBuilders(IDictionary<TypePair, TypeMap> typeMaps)
        {
            StorageBuilders.Add(new BeforeStorageBuilder(typeMaps));
            StorageBuilders.Add(new ConditionStorageBuilder(typeMaps));
        }



        private void RegisterTextBuilders(IDictionary<TypePair, TypeMap> typeMaps, MapperConfigurationExpression config)
        {
            //r.Add(x).With(y.With(a), z);

            TextBuilderRegistry
                .Add(new TextBuilder(typeMaps, config))
                .With(new CollectionTextBuilder(typeMaps, config), 
                    n => n.With(new OneArgTextBuilder(typeMaps, config))
                    )
                .With(new OneArgTextBuilder(typeMaps, config));
        }

        public  Dictionary<TypePair, CompiledDelegate> CompileMapsToAssembly(
            MapperConfigurationExpression config, 
            IDictionary<TypePair, TypeMap> typeMaps)
        {
            RegisterStorageBuilders(typeMaps);

            RegisterTextBuilders(typeMaps, config);

            var textBuilder = new TextBuilder(typeMaps, config);
            var files = textBuilder.CreateCodeFiles();
            Dictionary<TypePair, CodeFile> collectionFiles = new Dictionary<TypePair, CodeFile>();
            Dictionary<TypePair, CodeFile> oneArgFiles = new Dictionary<TypePair, CodeFile>();

            if (CreateCollectionMaps)
            {
                var ctb = new CollectionTextBuilder(typeMaps, config);
                collectionFiles = ctb.CreateCodeFiles(files);
            }

            var oatb = new OneArgTextBuilder(typeMaps, config);
            oneArgFiles = oatb.CreateCodeFiles(files);

            string[] sourceCodes = files.Values.Select(x => x.Code).ToArray();
            string[] collectionSourceCodes = collectionFiles.Values.Select(x => x.Code).ToArray();
            string[] oneArgSourceCodes = oneArgFiles.Values.Select(x => x.Code).ToArray();

            var storageCodes = BuildStorageCode();

            sourceCodes = sourceCodes
                .Union(storageCodes)
                .Union(collectionSourceCodes)
                .Union(oneArgSourceCodes)
                .ToArray();

            PrintSourceCode(sourceCodes);

            HashSet<string> locations = textBuilder.MethodInnerCodeBuilder.DetectedLocations; //TODO: REFACTOR!!!!

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(sourceCodes, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitStorages(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, files, collectionFiles, oneArgFiles, assembly);

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

        private  Dictionary<TypePair, CompiledDelegate> CreateDelegateCache(IDictionary<TypePair, TypeMap> typeMaps, 
            Dictionary<TypePair, CodeFile> singleFiles, 
            Dictionary<TypePair, CodeFile> collectionFiles, 
            Dictionary<TypePair, CodeFile> oneArgFiles, Assembly assembly)
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

                @delegate.SingleOneArg = CreateDelegate(map.MapDelegateTypeOneArg, assembly, oneArgFiles[typePair]);

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

            return typeof(Func<,,>).MakeGenericType(srcCollType, destCollType, destCollType);
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
        public object SingleOneArg { get; set; }
    }
}