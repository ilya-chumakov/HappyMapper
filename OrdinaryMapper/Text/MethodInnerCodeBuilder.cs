using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using AutoMapper.Extended.Net4;
using OrdinaryMapper.Saved;

namespace OrdinaryMapper
{
    public class MethodInnerCodeBuilder
    {
        public Dictionary<TypePair, string> TemplateCache { get; } = new Dictionary<TypePair, string>();
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }
        public IDictionary<TypePair, TypeMap> ImplicitTypeMaps { get; }
        public TypeMapFactory TypeMapFactory { get; } = new TypeMapFactory();
        public MapperConfigurationExpression Options { get; }
        public HashSet<string> DetectedLocations { get; } = new HashSet<string>();

        public MethodInnerCodeBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            ImplicitTypeMaps = explicitTypeMaps.ShallowCopy();
            Options = mce;
        }

        public Assignment CreateMethodInnerCode(TypeMap map, string srcFieldName ="src", string destFieldName = "dest")
        {
            RememberTypeLocations(map);

            var assignment = ProcessTypeMap(map, srcFieldName, destFieldName);

            return assignment;
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
                    var st = propertyMap.SrcType;
                    var dt = propertyMap.DestType;

                    if (dt.IsAssignableFrom(st) || dt.IsImplicitCastableFrom(st))
                    {
                        recorder.AppendAssignment(Assign.AsNoCast, context);
                        continue;
                    }
                    //assign with explicit cast
                    if (dt.IsExplicitCastableFrom(st))
                    {
                        recorder.AppendAssignment(Assign.AsExplicitCast, context);
                        continue;
                    }
                    //assign with src.ToString() call
                    if (dt == typeof(string) && st != typeof(string))
                    {
                        recorder.AppendAssignment(Assign.AsToStringCall, context);
                        continue;
                    }
                    //assign with Convert call
                    if (st == typeof(string) && dt.IsValueType)
                    {
                        recorder.AppendAssignment(Assign.AsStringToValueTypeConvert, context);
                        continue;
                    }

                    if (st.IsCollectionType() && dt.IsCollectionType())
                    {
                        var itemSrcType = st.GenericTypeArguments[0];
                        var itemDestType = dt.GenericTypeArguments[0];

                        //inner cycle variables (on each iteration itemSrcName is mapped to itemDestName).
                        string itemSrcName = "src_" + NamingTools.NewGuid(2);
                        string itemDestName = "dest_" + NamingTools.NewGuid(2);

                        var typePair = new TypePair(itemSrcType, itemDestType);

                        Assignment itemAssignment = new Assignment();

                        string cachedTemplate;
                        if (TemplateCache.TryGetValue(typePair, out cachedTemplate))
                        {
                            itemAssignment.Code = cachedTemplate.TemplateToCode(itemSrcName, itemDestName);
                            itemAssignment.RelativeTemplate = cachedTemplate;
                        }
                        else
                        {
                            var nodeMap = TypeMapFactory.CreateTypeMap(itemSrcType, itemDestType, Options);

                            itemAssignment = ProcessTypeMap(nodeMap, itemSrcName, itemDestName);
                        }

                        string code = CodeHelper.WrapForCode(itemAssignment.Code, 
                            new ForDeclarationContext(
                                context.SrcFullMemberName, context.DestFullMemberName, itemSrcName, itemDestName));

                        string template = CodeHelper.WrapForCode(itemAssignment.RelativeTemplate, 
                            new ForDeclarationContext(
                                "{0}", "{1}", itemSrcName, itemDestName));

                        Console.WriteLine("-----------------------------");
                        Console.WriteLine(code);
                        Console.WriteLine(template);
                        Console.WriteLine("-----------------------------");

                        recorder.AppendLine(code, template);
                    }

                    else
                    {
                        bool referenceType = dt.IsClass;
                        //TODO: perfomance degrades on each null check! Try to avoid it if possible!
                        if (referenceType)
                        {
                            recorder.NullCheck(context);
                            recorder.AppendRawCode(" else {{");

                            recorder.AppendNoParameterlessCtorException(context, dt);
                        }

                        ProcessPropertyTypePair(recorder, context, propertyMap);

                        if (referenceType) recorder.AppendRawCode("}}");
                    }
                }
            }

            var assignment = recorder.ToAssignment();

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
            string template;
            if (TemplateCache.TryGetValue(typePair, out template))
            {
                recorder.ApplyTemplate(template, context.SrcMemberName, context.DestMemberName);
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
            recorder.AppendPropertyAssignment(propAssignment, propertyMap);
            return;
        }
    }
}