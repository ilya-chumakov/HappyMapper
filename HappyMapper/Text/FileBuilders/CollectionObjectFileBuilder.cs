using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    /// <summary>
    /// Builds Map(object, object) wrap code for Map(ICollection<>, ICollection<>).
    /// </summary>
    internal class CollectionObjectFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public CollectionObjectFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
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
            @delegate.CollectionUntyped = Tools.CreateDelegate(
                Tools.ToCollectionUntypedDelegateType(map), assembly, file);
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

                //string filler = CreationTemplates.NewObject(typePair.DestinationType);
                var builder = new StringBuilder();
                builder.AppendLine($"var {cv.SrcParam} = {cv.SrcCollection} as {srcCollType};");
                builder.AppendLine($"var {cv.DestParam} = {cv.DestCollection} as {destCollType};");
                //builder.AppendLine($"{cv.DestParam}.Add({cv.SrcParam}.Count, () => {filler});");
                builder.AppendLine(CreationTemplates.Add(cv.DestParam, $"{cv.SrcParam}.Count", typePair.DestinationType));

                string methodCall = StatementTemplates.MethodCall(
                    mapCodeFile.GetClassAndMethodName(), cv.SrcParam, cv.DestParam);

                builder.AppendLine(methodCall);

                string methodCode = StatementTemplates.Method(builder.ToString(), 
                    new MethodDeclarationContext(cv.Method,
                        null,
                        new VariableContext(typeof(object).Name, cv.SrcCollection),
                        new VariableContext(typeof(object).Name, cv.DestCollection))
                    );

                var file = TextBuilderHelper.CreateFile(typePair, methodCode, cv.Method);

                files.Add(typePair, file);
            }

            return files.ToImmutableDictionary();
        }
    }
}