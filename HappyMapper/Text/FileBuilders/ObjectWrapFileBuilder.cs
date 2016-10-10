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
    /// Creates Map(object, object) wrap method for Map(ICollection<>, ICollection<>).
    /// </summary>
    public class ObjectWrapFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }

        public ObjectWrapFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression config)
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
            //@delegate.SingleOneArg = Tools.CreateDelegate(map.MapDelegateTypeOneArg, assembly, file);
            @delegate.CollectionUntyped = Tools.CreateDelegate(
                Tools.ToCollectionUntypedDelegateType(map), assembly, file);
        }

        public ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary(
            ImmutableDictionary<TypePair, CodeFile> parentFiles)
        {
            var files = new Dictionary<TypePair, CodeFile>();
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

                var mapCodeFile = parentFiles[typePair];

                var SrcTypeFullName = string.Format(template, typePair.SourceType.FullName.NormalizeTypeName());
                var DestTypeFullName = string.Format(template, typePair.DestinationType.FullName.NormalizeTypeName());

                string shortClassName = Convention.CreateUniqueMapperMethodNameWithGuid(typePair);
                string fullClassName = $"{Convention.Namespace}.{shortClassName}";

                string fill = Fill.GetText(typePair.DestinationType);
                var builder = new StringBuilder();
                builder.AppendLine($"var {srcParamName} = {srcCollectionName} as {SrcTypeFullName};");
                builder.AppendLine($"var {destParamName} = {destCollectionName} as {DestTypeFullName};");
                builder.AppendLine($"{destParamName}.Fill({srcParamName}.Count, () => {fill});");

                string methodCall = CodeTemplates.MethodCall(
                    mapCodeFile.GetClassAndMethodName(), srcParamName, destParamName);

                builder.AppendLine(methodCall);

                string methodCode = CodeTemplates.Method(builder.ToString(), 
                    new MethodDeclarationContext(methodName,
                        null,
                        new VariableContext(typeof(object).Name, srcCollectionName),
                        new VariableContext(typeof(object).Name, destCollectionName))
                    );

                string classCode = CodeTemplates.Class(methodCode, Convention.Namespace, shortClassName);

                var file = new CodeFile(classCode, fullClassName, methodName, typePair, default(Assignment));

                files.Add(typePair, file);
            }

            return files.ToImmutableDictionary();
        }
    }
}