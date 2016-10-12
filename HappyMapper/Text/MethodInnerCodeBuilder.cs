using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using AutoMapper.Extended.Net4;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    internal class MethodInnerCodeBuilder
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

            using (var bfm = new BeforeMapPrinter(new TypeNameContext(rootMap), recorder)) { }

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

                    if (!st.IsValueType && !dt.IsValueType)
                    {
                        using (var block = new Block(recorder, "if", $"{{0}}.{ctx.SrcMemberName} == null"))
                        {
                            recorder.AppendLine($"{{1}}.{ctx.DestMemberName} = null;");
                        }

                        using (var block = new Block(recorder, "else"))
                        {
                            if (st.IsCollectionType() && dt.IsCollectionType())
                            {
                                string template = AssignCollections(ctx)
                                    .AddPropertyNamesToTemplate(ctx.SrcMemberName, ctx.DestMemberName);

                                recorder.AppendLine(template);
                            }

                            else
                            {
                                string template = AssignReferenceTypes(ctx)
                                    .AddPropertyNamesToTemplate(ctx.SrcMemberName, ctx.DestMemberName);

                                recorder.AppendLine(template);
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            var assignment = recorder.ToAssignment();

            TemplateCache.AddIfNotExist(rootMap.TypePair, assignment.RelativeTemplate);

            return assignment;
        }

        private string AssignCollections(IPropertyNameContext ctx)
        {
            var recorder = new Recorder();

            var itemSrcType = ctx.SrcType.GenericTypeArguments[0];
            var itemDestType = ctx.DestType.GenericTypeArguments[0];

            using (var block = new Block(recorder, "if", "{1} == null"))
            {
                string newCollection = CreationTemplates.NewCollection(ctx.DestType, "{0}.Count");
                recorder.AppendLine($"{{1}} = {newCollection};");
            }
            //, "{1} == null || {0}.Count != {1}.Count"
            using (var block = new Block(recorder, "else"))
            {
                recorder.AppendLine("{1}.Clear();");
            }
            //fill new (or cleared) collection with the new set of items
            recorder.AppendLine(CreationTemplates.Add("{1}", "{0}.Count", itemDestType));

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
            else if (itemSrcType.IsCollectionType() && itemDestType.IsCollectionType())
            {
                var innerContext = PropertyNameContextFactory.CreateWithoutPropertyMap(
                    itemSrcType, itemDestType, itemSrcName, itemDestName);

                string innerTemplate = AssignCollections(innerContext);

                itemAssignment.RelativeTemplate = innerTemplate;
            }
            else
            {
                var nodeMap = GetTypeMap(typePair);

                itemAssignment = ProcessTypeMap(nodeMap);
            }

            string iterationCode = itemAssignment.GetCode(itemSrcName, itemDestName);

            string forCode = StatementTemplates.For(iterationCode,
                new ForDeclarationContext( "{0}", "{1}", itemSrcName, itemDestName));

            recorder.AppendLine(forCode);

            string template = recorder.ToAssignment().RelativeTemplate;

            return template;
        }

        private string AssignReferenceTypes(PropertyNameContext ctx)
        {
            Recorder recorder = new Recorder();

            //has parameterless ctor
            if (ctx.DestType.HasParameterlessCtor())
            {
                //create new Dest() object
                string newDest = $"{{1}} = {StatementTemplates.New(ctx.DestTypeFullName)};";

                recorder.AppendLine(newDest);
            }
            else
            {
                throw new HappyMapperException(ErrorMessages.NoParameterlessCtor(ctx.DestType));
            }

            string template;
            //typepair isn't in template cache
            if (!TemplateCache.TryGetValue(ctx.TypePair, out template))
            {
                var nodeMap = GetTypeMap(ctx.TypePair);

                var assignment = ProcessTypeMap(nodeMap);

                template = assignment.RelativeTemplate
                    //.AddPropertyNamesToTemplate(ctx.SrcMemberName, ctx.DestMemberName)
                    ;
            }

            recorder.AppendLine(template);

            return recorder.ToAssignment().RelativeTemplate;
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

        private TypeMap GetTypeMap(TypePair typePair)
        {
            TypeMap nodeMap;

            //typepair explicitly mapped by user
            ExplicitTypeMaps.TryGetValue(typePair, out nodeMap);

            if (nodeMap == null)
            {
                if (!ImplicitTypeMaps.TryGetValue(typePair, out nodeMap))
                {
                    //create implicit map 
                    nodeMap = TypeMapFactory.CreateTypeMap(typePair.SourceType, typePair.DestinationType, Options);
                    ImplicitTypeMaps.AddIfNotExist(nodeMap);
                }
            }
            return nodeMap;
        }
    }
}