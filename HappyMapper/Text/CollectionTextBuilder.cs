using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    public class CollectionTextBuilder : ITextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public CollectionTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles(Dictionary<TypePair, CodeFile> files)
        {
            return CreateCodeFiles(files.ToImmutableDictionary());
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles(ImmutableDictionary<TypePair, CodeFile> files)
        {
            var collectionFiles = new Dictionary<TypePair, CodeFile>();
            var Convention = NameConventionsStorage.Mapper;

            //TODO: move to convention
            string srcParamName = "src";
            string destParamName = "dest";
            string srcCollectionName = "srcList";
            string destCollectionName = "destList";
            string methodName = "MapCollection";
            string template = "ICollection<{0}>";

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mapCodeFile = files[typePair];

                var SrcTypeFullName = string.Format(template, typePair.SourceType.FullName);
                var DestTypeFullName = string.Format(template, typePair.DestinationType.FullName);

                string shortClassName = Convention.CreateUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                string methodInnerCode = mapCodeFile.InnerMethodAssignment
                    .GetCode(srcParamName, destParamName)
                    .RemoveDoubleBraces();

                var forCode = CodeTemplates.For(methodInnerCode,
                    new ForDeclarationContext(srcCollectionName, destCollectionName, srcParamName, destParamName));

                string methodCode = CodeTemplates.Method(forCode, 
                    new MethodDeclarationContext(methodName,
                        new VariableContext(DestTypeFullName, destCollectionName), 
                        new VariableContext(SrcTypeFullName, srcCollectionName),
                        new VariableContext(DestTypeFullName, destCollectionName)));

                string classCode = CodeTemplates.Class(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, default(Assignment));

                collectionFiles.Add(typePair, file);
            }

            return collectionFiles;
        }

        public HashSet<string> GetLocations()
        {
            return new HashSet<string>();
        }

        public void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly, CodeFile file)
        {
            @delegate.Collection = Tools.CreateDelegate(Tools.ToCollectionDelegateType(map), assembly, file);
        }
    }
}