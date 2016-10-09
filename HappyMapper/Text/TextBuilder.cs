using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace HappyMapper.Text
{
    public class TextBuilder : ITextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public TextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();

            MethodInnerCodeBuilder = new MethodInnerCodeBuilder(explicitTypeMaps, mce);
        }

        public MethodInnerCodeBuilder MethodInnerCodeBuilder { get; set; }

        public HashSet<string> GetLocations()
        {
            return MethodInnerCodeBuilder.DetectedLocations;
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles()
        {
            var files = new Dictionary<TypePair, CodeFile>();
            var Convention = NameConventionsStorage.Mapper;

            //TODO: move to convention
            string srcFieldName = "src";
            string destFieldName = "dest";
            string methodName = "Map";

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var SrcTypeFullName = typePair.SourceType.FullName;
                var DestTypeFullName = typePair.DestinationType.FullName;

                string shortClassName = Convention.CreateUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                var assignment = MethodInnerCodeBuilder.GetAssignment(map);

                string methodInnerCode = assignment.GetCode(srcFieldName, destFieldName);

                string methodCode = CodeTemplates.Method(methodInnerCode, 
                    new MethodDeclarationContext(methodName,
                        new VariableContext(DestTypeFullName, destFieldName),
                        new VariableContext(SrcTypeFullName, srcFieldName),
                        new VariableContext(DestTypeFullName, destFieldName)));

                string classCode = CodeTemplates.Class(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, assignment);

                files.Add(typePair, file);
            }

            return files;
        }
    }
}