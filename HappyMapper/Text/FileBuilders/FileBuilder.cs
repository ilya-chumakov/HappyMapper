using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    public class FileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public FileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();

            MethodInnerCodeBuilder = new MethodInnerCodeBuilder(explicitTypeMaps, mce);
        }

        public MethodInnerCodeBuilder MethodInnerCodeBuilder { get; set; }

        public HashSet<string> GetLocations()
        {
            return MethodInnerCodeBuilder.DetectedLocations;
        }

        public void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly, CodeFile file)
        {
            @delegate.Single = Tools.CreateDelegate(map.MapDelegateType, assembly, file);
        }

        public TextResult Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            var files = CreateCodeFilesDictionary();

            return new TextResult(files, MethodInnerCodeBuilder.DetectedLocations);
        }

        private ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary()
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

            return files.ToImmutableDictionary();
        }
    }
}