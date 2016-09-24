using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
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

                string methodCode = CreateMethodInnerCode(map);
                methodCode = methodCode.Replace("{{", "").Replace("}}", "");
                var fileBuilder = new CodeFileBuilder(typePair);

                var file = fileBuilder.CreateCodeFile(methodCode);

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
            var coder = new Coder();

            using (var bfm = new BeforeMapActionBuilder(rootMap, coder, srcFieldName, destFieldName))
            {
                
            }

            foreach (PropertyMap propertyMap in rootMap.PropertyMaps)
            {
                RememberTypeLocations(propertyMap);

                var context = new PropertyNameContext(propertyMap, srcFieldName, destFieldName);

                if (propertyMap.Ignored) continue;

                //using (var condition = new ConditionBuilder(context, coder))
                using (var condition = new ConditionBuilderV2(context, coder))
                {
                    //assign without explicit cast
                    if (propertyMap.DestType.IsAssignableFrom(propertyMap.SrcType)
                        || propertyMap.DestType.IsImplicitCastableFrom(propertyMap.SrcType))
                    {
                        //TODO: need to determine explicit casts and produce cast operators
                        coder.SimpleAssign(context);
                        continue;
                    }
                    else
                    {
                        bool referenceType = propertyMap.DestType.IsClass;
                        //TODO: perfomance degrades on each null check! Try to avoid it if possible!
                        if (referenceType)
                        {
                            coder.NullCheck(context);
                            coder.AttachRawCode(" else {{");

                            coder.AppendNoParameterlessCtorException(context, propertyMap.DestType);
                        }

                        ProcessPropertyTypePair(coder, context, propertyMap);

                        if (referenceType) coder.AttachRawCode("}}");
                    }
                }
            }

            var assignment = coder.GetAssignment();

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

        private void ProcessPropertyTypePair(Coder coder, PropertyNameContext context, PropertyMap propertyMap)
        {
            var typePair = propertyMap.GetTypePair();

            //typepair already in template cache
            string text;
            if (TemplateCache.TryGetValue(typePair, out text))
            {
                coder.ApplyTemplate(context, text);
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
            coder.AttachPropertyAssignment(propAssignment, propertyMap);
            return;
        }
    }
}