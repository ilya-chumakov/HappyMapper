using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace HappyMapper.Text
{
    public class OneArgTextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public OneArgTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
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

            return collectionFiles;
        }
    }
}