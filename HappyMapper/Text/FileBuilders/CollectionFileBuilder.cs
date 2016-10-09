using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    public class CollectionFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public CollectionFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
        }

        public TextResult Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            var files = CreateCodeFilesDictionary(parentFiles);

            return new TextResult(files, new HashSet<string>());
        }

        public void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly, CodeFile file)
        {
            @delegate.Collection = Tools.CreateDelegate(Tools.ToCollectionDelegateType(map), assembly, file);
        }

        public ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary(
            ImmutableDictionary<TypePair, CodeFile> files)
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

            return collectionFiles.ToImmutableDictionary();
        }
    }
}