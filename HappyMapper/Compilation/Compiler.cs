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
        private TextBuilderRunner TextBuilderRunner { get; set; } = new TextBuilderRunner();

        private List<IStorageBuilder> StorageBuilders { get; set; } = new List<IStorageBuilder>();
        private bool CreateCollectionMaps { get; } = true;


        void RegisterStorageBuilders(IDictionary<TypePair, TypeMap> typeMaps)
        {
            StorageBuilders.Add(new BeforeStorageBuilder(typeMaps));
            StorageBuilders.Add(new ConditionStorageBuilder(typeMaps));
        }

        private void RegisterTextBuilders(IDictionary<TypePair, TypeMap> typeMaps, MapperConfigurationExpression config)
        {
            //r.Add(x).With(y.With(a), z);

            TextBuilderRunner
                .Add(new TextBuilder(typeMaps, config))
                .With(new CollectionTextBuilder(typeMaps, config)
                    //, n => n.With(new OneArgTextBuilder(typeMaps, config))
                    )
                .With(new OneArgTextBuilder(typeMaps, config));
        }

        public Dictionary<TypePair, CompiledDelegate> CompileMapsToAssembly(
            MapperConfigurationExpression config,
            IDictionary<TypePair, TypeMap> typeMaps)
        {
            RegisterStorageBuilders(typeMaps);

            RegisterTextBuilders(typeMaps, config);

            List<string> sourceCodes;
            HashSet<string> locations;
            TextBuilderRunner.GetCode(out sourceCodes, out locations);

            var storageCodes = BuildStorageCode();

            sourceCodes = sourceCodes
                .Union(storageCodes)
                .ToList();

            PrintSourceCode(sourceCodes);

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(sourceCodes.ToArray(), locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            InitStorages(typeMaps, assembly);

            var delegateCache = CreateDelegateCache(typeMaps, assembly);

            return delegateCache;
        }

        private void InitStorages(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
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

        private Dictionary<TypePair, CompiledDelegate> CreateDelegateCache(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var cache = new Dictionary<TypePair, CompiledDelegate>();

            foreach (var kvp in typeMaps)
            {
                var @delegate = new CompiledDelegate();

                TypeMap map = kvp.Value;
                TypePair typePair = kvp.Key;

                foreach (var node in TextBuilderRunner.AllRules.Select(node => node))
                {
                    node.Builder.VisitDelegate(@delegate, map, assembly, node.Result.Files[typePair]);
                }

                //@delegate.Single = CreateDelegate(map.MapDelegateType, assembly, singleFiles[typePair]);

                //if (CreateCollectionMaps)
                //    @delegate.Collection = CreateDelegate(ToCollectionDelegateType(map), assembly, collectionFiles[typePair]);

                //@delegate.SingleOneArg = CreateDelegate(map.MapDelegateTypeOneArg, assembly, oneArgFiles[typePair]);

                cache.Add(typePair, @delegate);
            }
            return cache;
        }
    }
}