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
        public IDictionary<TypePair, TypeMap> TypeMaps { get; private set; }
        public MapperConfigurationExpression Config { get; private set; }
        private FileBuilderRunner FileBuilderRunner { get; set; } = new FileBuilderRunner();
        private List<IStorageBuilder> StorageBuilders { get; set; } = new List<IStorageBuilder>();
        private bool CreateCollectionMaps { get; } = true;

        public Compiler(
            MapperConfigurationExpression config,
            IDictionary<TypePair, TypeMap> typeMaps)
        {
            Config = config;
            TypeMaps = typeMaps;

            RegisterFileBuilders();
            RegisterStorageBuilders();
        }

        void RegisterStorageBuilders()
        {
            StorageBuilders.Add(new BeforeStorageBuilder(TypeMaps));
            StorageBuilders.Add(new ConditionStorageBuilder(TypeMaps));
        }

        private void RegisterFileBuilders()
        {
            FileBuilderRunner
                .Add(new SingleFileBuilder(TypeMaps, Config))
                .With(new CollectionFileBuilder(TypeMaps, Config)
                    , n => n.With(new CollectionObjectFileBuilder(TypeMaps, Config))
                    )
                .With(new SingleOneArgFileBuilder(TypeMaps, Config));
        }

        public Dictionary<TypePair, CompiledDelegate> CompileMapsToAssembly()
        {
            List<string> sourceCodes;
            HashSet<string> locations;
            FileBuilderRunner.BuildCode(out sourceCodes, out locations);

            var storageCodes = BuildStorageCode();

            sourceCodes = sourceCodes.Union(storageCodes).ToList();

            PrintSourceCode(sourceCodes);

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(sourceCodes.ToArray(), locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitStorages(TypeMaps, assembly);

            var delegateCache = CreateDelegateCache(TypeMaps, assembly);

            return delegateCache;
        }

        private void InitStorages(IDictionary<TypePair, TypeMap> TypeMaps, Assembly assembly)
        {
            StorageBuilders.ForEach(b => b.InitStorage(assembly));
        }

        private string[] BuildStorageCode()
        {
            return StorageBuilders.Select(builder => builder.BuildCode())
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToArray();
        }

        private void PrintSourceCode(ICollection<string> sourceCodes)
        {
            foreach (string code in sourceCodes)
            {
                Debug.WriteLine(code);
            }
        }

        private Dictionary<TypePair, CompiledDelegate> CreateDelegateCache(IDictionary<TypePair, TypeMap> TypeMaps, Assembly assembly)
        {
            var cache = new Dictionary<TypePair, CompiledDelegate>();

            foreach (var kvp in TypeMaps)
            {
                var @delegate = new CompiledDelegate();

                TypeMap map = kvp.Value;

                foreach (var rule in FileBuilderRunner.AllRules)
                {
                    rule.VisitDelegate(@delegate, map, assembly);
                }

                cache.Add(map.TypePair, @delegate);
            }
            return cache;
        }
    }
}