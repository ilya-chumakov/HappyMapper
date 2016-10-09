using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    public class OneArgFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public OneArgFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
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
            @delegate.SingleOneArg = Tools.CreateDelegate(map.MapDelegateTypeOneArg, assembly, file);
        }

        public ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary(
            ImmutableDictionary<TypePair, CodeFile> files)
        {
            var collectionFiles = new Dictionary<TypePair, CodeFile>();
            var Convention = NameConventionsStorage.Mapper;

            //TODO: move to convention
            string srcParamName = "src";
            string destParamName = "dest";
            string methodName = "Map";

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mapCodeFile = files[typePair];

                var SrcTypeFullName = typePair.SourceType.FullName.NormalizeTypeName();
                var DestTypeFullName = typePair.DestinationType.FullName.NormalizeTypeName();

                string shortClassName = Convention.CreateUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                string arg1 = $"{srcParamName} as {SrcTypeFullName}";
                string arg2 = CodeTemplates.New(DestTypeFullName);

                var methodCall = CodeTemplates.MethodCall(mapCodeFile.GetClassAndMethodName(), arg1, arg2);

                string methodCode = CodeTemplates.Method(string.Empty, 
                    new MethodDeclarationContext(methodName,
                        new VariableContext(DestTypeFullName, methodCall),
                        new VariableContext(typeof(object).Name, srcParamName)));

                string classCode = CodeTemplates.Class(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, default(Assignment));

                collectionFiles.Add(typePair, file);
            }

            return collectionFiles.ToImmutableDictionary();
        }
    }
}