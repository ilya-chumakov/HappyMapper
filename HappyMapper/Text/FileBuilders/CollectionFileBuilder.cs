using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    /// <summary>
    /// Builds Map(ICollection<>, ICollection<>) code.
    /// </summary>
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
            ImmutableDictionary<TypePair, CodeFile> parentFiles)
        {
            var files = new Dictionary<TypePair, CodeFile>();
            var cv = NameConventionsStorage.MapCollection;

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;

                var mapCodeFile = parentFiles[typePair];

                string srcCollType = cv.GetCollectionType(typePair.SourceType.FullName);
                string destCollType = cv.GetCollectionType(typePair.DestinationType.FullName);

                string methodInnerCode = mapCodeFile.InnerMethodAssignment
                    .GetCode(cv.SrcParam, cv.DestParam)
                    .RemoveDoubleBraces();

                var forCode = StatementTemplates.For(methodInnerCode,
                    new ForDeclarationContext(cv.SrcCollection, cv.DestCollection, cv.SrcParam, cv.DestParam));

                string methodCode = StatementTemplates.Method(forCode, 
                    new MethodDeclarationContext(cv.Method,
                        new VariableContext(destCollType, cv.DestCollection), 
                        new VariableContext(srcCollType, cv.SrcCollection),
                        new VariableContext(destCollType, cv.DestCollection)));

                var file = TextBuilderHelper.CreateFile(typePair, methodCode, cv.Method);

                files.Add(typePair, file);
            }

            return files.ToImmutableDictionary();
        }
    }
}