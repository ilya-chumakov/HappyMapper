using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;

namespace OrdinaryMapper
{
    public class CollectionTextBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public CollectionTextBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles(Dictionary<TypePair, CodeFile> files)
        {
            return new Dictionary<TypePair, CodeFile>();
            return CreateCodeFiles(files.ToImmutableDictionary());
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles(ImmutableDictionary<TypePair, CodeFile> xfiles)
        {
            var collectionFiles = new Dictionary<TypePair, CodeFile>();
            var Convention = NameConventions.Mapper;

            //TODO: move to convention
            string srcFieldName = "src";
            string destFieldName = "dest";
            string methodName = "Map";

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                var mapCodeFile = xfiles[typePair];


                string template = "ICollection<{0}>";

                var SrcTypeFullName = string.Format(template, typePair.SourceType.FullName);
                var DestTypeFullName = string.Format(template, typePair.DestinationType.FullName);

                string shortClassName = Convention.GetUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                string methodInnerCode = mapCodeFile.InnerMethodAssignment.Code.RemoveDoubleBraces();

                //TODO: insert field names
                //string methodCode = CodeHelper.WrapMethodCode(methodInnerCode, methodName, SrcTypeFullName, DestTypeFullName);
                string methodCode = CodeHelper.WrapCollectionCode(methodInnerCode, methodName, SrcTypeFullName, DestTypeFullName);

                string classCode = CodeHelper.BuildClassCode(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, default(Assignment));

                collectionFiles.Add(typePair, file);
            }

            return collectionFiles;
        }
    }
}