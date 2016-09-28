using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace OrdinaryMapper
{
    public class MapperTextBuilderV2
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public MapperTextBuilderV2(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();

            MethodInnerCodeBuilder = new MethodInnerCodeBuilder(explicitTypeMaps, mce);
        }

        public MethodInnerCodeBuilder MethodInnerCodeBuilder { get; set; }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles()
        {
            var files = new Dictionary<TypePair, CodeFile>();
            var Convention = NameConventions.Mapper;

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

                string shortClassName = Convention.GetUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                var assignment = MethodInnerCodeBuilder.CreateMethodInnerCode(map, srcFieldName, destFieldName);

                string methodInnerCode = assignment.Code.RemoveDoubleBraces();

                string methodCode = CodeHelper.WrapMethodCode(methodInnerCode, 
                    new MethodDeclarationContext(methodName, SrcTypeFullName, DestTypeFullName, srcFieldName, destFieldName));

                string classCode = CodeHelper.BuildClassCode(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, assignment);

                files.Add(typePair, file);
            }

            return files;
        }
    }

    public class MethodDeclarationContext
    {
        public string DestParam { get; }
        public string DestType { get; }
        public string MethodName { get; }
        public string SrcParam { get; }
        public string SrcType { get; }

        public MethodDeclarationContext(string methodName, string srcType, string destType, string srcParam, string destFieldName)
        {
            this.MethodName = methodName;
            this.SrcType = srcType.NormalizeTypeName();
            this.DestType = destType.NormalizeTypeName();
            this.SrcParam = srcParam;
            this.DestParam = destFieldName;
        }
    }
}