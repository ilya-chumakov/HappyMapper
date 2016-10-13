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
    /// Builds Map(object) wrap code for the Map(Src, Dest) method.
    /// </summary>
    internal class SingleOneArgFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public SingleOneArgFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
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
            @delegate.SingleUntyped = Tools.CreateDelegate(map.DelegateTypeSingleUntyped, assembly, file);
        }

        public ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary(
            ImmutableDictionary<TypePair, CodeFile> parentFiles)
        {
            var files = new Dictionary<TypePair, CodeFile>();
            var cv = NameConventionsStorage.Map;

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;

                var mapCodeFile = parentFiles[typePair];

                string srcType = typePair.SourceType.FullName.NormalizeTypeName();
                string destType = typePair.DestinationType.FullName.NormalizeTypeName();

                string arg1 = $"{cv.SrcParam} as {srcType}";
                string arg2 = StatementTemplates.New(destType);

                var methodCall = StatementTemplates.MethodCall(mapCodeFile.GetClassAndMethodName(), arg1, arg2);

                string methodCode = StatementTemplates.Method(string.Empty, 
                    new MethodDeclarationContext(cv.Method,
                        new VariableContext(destType, methodCall),
                        new VariableContext(typeof(object).Name, cv.SrcParam)));

                var file = TextBuilderHelper.CreateFile(typePair, methodCode, cv.Method);

                files.Add(typePair, file);
            }

            return files.ToImmutableDictionary();
        }
    }
}