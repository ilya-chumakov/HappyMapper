using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

        public Assignment GetAssignment(TypeMap map)
        {
            RememberTypeLocations(map);

            var assignment = ProcessTypeMap(map);

            return assignment;
        }

        private Assignment ProcessTypeMap(TypeMap rootMap)
        {
            var recorder = new Recorder();

            using (var bfm = new BeforeMapPrinter(new TypeNameContext(rootMap), recorder)) {}

            foreach (PropertyMap propertyMap in rootMap.PropertyMaps)
            {
                if (propertyMap.Ignored) continue;

                RememberTypeLocations(propertyMap);

                var ctx = new PropertyNameContext(propertyMap);

                //using (var condition = new ConditionPrinter(context, Recorder))
                using (var condition = new ConditionPrinterV2(ctx, recorder))
                {
                    //assign without explicit cast
                    var st = propertyMap.SrcType;
                    var dt = propertyMap.DestType;

                    if (dt.IsAssignableFrom(st) || dt.IsImplicitCastableFrom(st))
                    {
                        recorder.AppendAssignment(Assign.AsNoCast, ctx);
                        continue;
                    }
                    //assign with explicit cast
                    if (dt.IsExplicitCastableFrom(st))
                    {
                        recorder.AppendAssignment(Assign.AsExplicitCast, ctx);
                        continue;
                    }
                    //assign with src.ToString() call
                    if (dt == typeof(string) && st != typeof(string))
                    {
                        recorder.AppendAssignment(Assign.AsToStringCall, ctx);
                        continue;
                    }
                    //assign with Convert call
                    if (st == typeof(string) && dt.IsValueType)
                    {
                        recorder.AppendAssignment(Assign.AsStringToValueTypeConvert, ctx);
                        continue;
                    }

                    if (st.IsCollectionType() && dt.IsCollectionType())
                    {
                        var itemSrcType = st.GenericTypeArguments[0];
                        var itemDestType = dt.GenericTypeArguments[0];

                        //inner cycle variables (on each iteration itemSrcName is mapped to itemDestName).
                        string itemSrcName = "src_" + NamingTools.NewGuid(4);
                        string itemDestName = "dest_" + NamingTools.NewGuid(4);

                        var typePair = new TypePair(itemSrcType, itemDestType);

                        Assignment itemAssignment = new Assignment();

                        string cachedTemplate;
                        if (TemplateCache.TryGetValue(typePair, out cachedTemplate))
                        {
                            itemAssignment.RelativeTemplate = cachedTemplate;
                        }
                        else
                        {
                            var nodeMap = TypeMapFactory.CreateTypeMap(itemSrcType, itemDestType, Options);

                            itemAssignment = ProcessTypeMap(nodeMap);
                        }

                        string iterationCode = itemAssignment.GetCode(itemSrcName, itemDestName);

                        string template = CodeTemplates.For(iterationCode, 
                            new ForDeclarationContext(
                                "{0}", "{1}", itemSrcName, itemDestName));

                        template = template.AddPropertyNamesToTemplate(ctx.SrcMemberName, ctx.DestMemberName);

                        Debug.WriteLine("-----------------------------");
                        Debug.WriteLine(template);
                        Debug.WriteLine("-----------------------------");

                        recorder.AppendLine(template);
                    }

                    else
                    {
                        bool referenceType = dt.IsClass;
                        //TODO: perfomance degrades on each null check! Try to avoid it if possible!
                        if (referenceType)
                        {
                            string template = CodeTemplates.NullCheck("{0}", "{1}");
                            recorder.AppendLine(template);
                            recorder.AppendRawCode(" else {{");

                            recorder.AppendNoParameterlessCtorException(ctx, dt);
                        }

                        ProcessPropertyMap(recorder, ctx, propertyMap);

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

        private void ProcessPropertyMap(Recorder recorder, PropertyNameContext context, PropertyMap propertyMap)
        {
            var typePair = propertyMap.GetTypePair();

            //typepair already in template cache
            string template;
            if (TemplateCache.TryGetValue(typePair, out template))
            {
                recorder.AppendLine(template);
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

            var assignment = ProcessTypeMap(nodeMap);

            string shiftedTemplate = assignment.RelativeTemplate.AddPropertyNamesToTemplate(
                propertyMap.SrcMember.Name, propertyMap.DestMember.Name);

            recorder.AppendLine(shiftedTemplate);
            return;
        }
    }
}