using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using OrdinaryMapper.Saved;

namespace OrdinaryMapper
{
    public class MapperTextBuilderV2
    {
        public Dictionary<TypePair, string> TemplateCache { get; } = new Dictionary<TypePair, string>();
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }
        public IDictionary<TypePair, TypeMap> ImplicitTypeMaps { get; }
        public TypeMapFactory TypeMapFactory { get; } = new TypeMapFactory();
        public MapperConfigurationExpression Options { get; }
        public HashSet<string> DetectedLocations { get; } = new HashSet<string>();

        public MapperTextBuilderV2(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            ImplicitTypeMaps = explicitTypeMaps.ShallowCopy();
            Options = mce;
        }

        public Dictionary<TypePair, CodeFile> CreateCodeFiles()
        {
            var files = new Dictionary<TypePair, CodeFile>();

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                string methodInnerCode = CreateMethodInnerCode(map).RemoveDoubleBraces();

                var fileBuilder = new CodeFileBuilder(typePair);

                var file = fileBuilder.CreateCodeFile(methodInnerCode);

                files.Add(typePair, file);
            }

            return files;
        }

        public string CreateMethodInnerCode(TypeMap map)
        {
            RememberTypeLocations(map);

            string srcFieldName = "src";
            string destFieldName = "dest";

            var assignment = ProcessTypeMap(map, srcFieldName, destFieldName);

            return assignment.Code;
        }

        private Assignment ProcessTypeMap(TypeMap rootMap, string srcFieldName, string destFieldName)
        {
            var recorder = new Recorder();

            using (var bfm = new BeforeMapPrinter(new TypeNameContext(rootMap, srcFieldName, destFieldName), recorder)) {}

            foreach (PropertyMap propertyMap in rootMap.PropertyMaps)
            {
                RememberTypeLocations(propertyMap);

                var context = new PropertyNameContext(propertyMap, srcFieldName, destFieldName);

                if (propertyMap.Ignored) continue;

                //using (var condition = new ConditionPrinter(context, Recorder))
                using (var condition = new ConditionPrinterV2(context, recorder))
                {
                    //assign without explicit cast
                    if (propertyMap.DestType.IsAssignableFrom(propertyMap.SrcType)
                        || propertyMap.DestType.IsImplicitCastableFrom(propertyMap.SrcType))
                    {
                        recorder.AssignAsNoCast(context);
                        continue;
                    }
                    //assign with explicit cast
                    if (propertyMap.DestType.IsExplicitCastableFrom(propertyMap.SrcType))
                    {
                        recorder.AssignAsExplicitCast(context);
                        continue;
                    }
                    //assign with src.ToString() call
                    if (propertyMap.DestType == typeof(string) && propertyMap.SrcType != typeof(string))
                    {
                        recorder.AssignAsToStringCall(context);
                        continue;
                    }
                    //assign with Convert call
                    if (propertyMap.SrcType == typeof(string) && propertyMap.DestType.IsValueType)
                    {
                        recorder.AssignAsStringToValueTypeConvert(context);
                        continue;
                    }

                    else
                    {
                        bool referenceType = propertyMap.DestType.IsClass;
                        //TODO: perfomance degrades on each null check! Try to avoid it if possible!
                        if (referenceType)
                        {
                            recorder.NullCheck(context);
                            recorder.AttachRawCode(" else {{");

                            recorder.AppendNoParameterlessCtorException(context, propertyMap.DestType);
                        }

                        ProcessPropertyTypePair(recorder, context, propertyMap);

                        if (referenceType) recorder.AttachRawCode("}}");
                    }
                }
            }

            var assignment = recorder.GetAssignment();

            TemplateCache.AddIfNotExist(rootMap.TypePair, assignment.RelativeTemplate);

            return assignment;
        }

        /// <summary>
        /// remember all used types
        /// </summary>
        /// <param name="propertyMap"></param>
        private void RememberTypeLocations(PropertyMap propertyMap)
        {
            DetectedLocations.Add(propertyMap.SrcType.Assembly.Location);
            DetectedLocations.Add(propertyMap.DestType.Assembly.Location);
        }

        private void RememberTypeLocations(TypeMap typeMap)
        {
            DetectedLocations.Add(typeMap.SourceType.Assembly.Location);
            DetectedLocations.Add(typeMap.DestinationType.Assembly.Location);
        }

        private void ProcessPropertyTypePair(Recorder recorder, PropertyNameContext context, PropertyMap propertyMap)
        {
            var typePair = propertyMap.GetTypePair();

            //typepair already in template cache
            string text;
            if (TemplateCache.TryGetValue(typePair, out text))
            {
                recorder.ApplyTemplate(context, text);
                return;
            }

            TypeMap nodeMap;

            //typepair explicitly mapped by user
            ExplicitTypeMaps.TryGetValue(typePair, out nodeMap);

            if (nodeMap == null)
            {
                if (!ImplicitTypeMaps.TryGetValue(typePair, out nodeMap))
                {
                    //create implicit map 
                    nodeMap = TypeMapFactory.CreateTypeMap(propertyMap.SrcType, propertyMap.DestType, Options);
                    ImplicitTypeMaps.AddIfNotExist(nodeMap);
                }
            }

            var propAssignment = ProcessTypeMap(nodeMap, context.SrcFullMemberName, context.DestFullMemberName);
            recorder.AttachPropertyAssignment(propAssignment, propertyMap);
            return;
        }
    }
}