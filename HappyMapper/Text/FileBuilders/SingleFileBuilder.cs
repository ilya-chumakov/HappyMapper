using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using AutoMapper.ConfigurationAPI;
using AutoMapper.ConfigurationAPI.Configuration;
using HappyMapper.Compilation;

namespace HappyMapper.Text
{
    /// <summary>
    /// Builds Map(Src, Dest) method code.
    /// </summary>
    internal class SingleFileBuilder : IFileBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; }
        public MethodInnerCodeBuilder MethodInnerCodeBuilder { get; set; }

        public SingleFileBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps, MapperConfigurationExpression mce)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();

            MethodInnerCodeBuilder = new MethodInnerCodeBuilder(explicitTypeMaps, mce);
        }

        public HashSet<string> GetLocations()
        {
            return MethodInnerCodeBuilder.DetectedLocations;
        }

        public void VisitDelegate(CompiledDelegate @delegate, TypeMap map, Assembly assembly, CodeFile file)
        {
            @delegate.SingleTyped = Tools.CreateDelegate(map.DelegateTypeSingleTyped, assembly, file);
        }

        public TextResult Build(ImmutableDictionary<TypePair, CodeFile> parentFiles = null)
        {
            var files = CreateCodeFilesDictionary();

            return new TextResult(files, MethodInnerCodeBuilder.DetectedLocations);
        }

        private ImmutableDictionary<TypePair, CodeFile> CreateCodeFilesDictionary()
        {
            var files = new Dictionary<TypePair, CodeFile>();
            var cv = NameConventionsStorage.Map;

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                string srcType = typePair.SourceType.FullName.NormalizeTypeName();
                string destType = typePair.DestinationType.FullName.NormalizeTypeName();

                var assignment = MethodInnerCodeBuilder.GetAssignment(map);

                string methodInnerCode = assignment.GetCode(cv.SrcParam, cv.DestParam);

                string methodCode = StatementTemplates.Method(methodInnerCode, 
                    new MethodDeclarationContext(cv.Method,
                        new VariableContext(destType, cv.DestParam),
                        new VariableContext(srcType, cv.SrcParam),
                        new VariableContext(destType, cv.DestParam)));

                var file = TextBuilderHelper.CreateFile(typePair, methodCode, cv.Method, assignment);

                files.Add(typePair, file);
            }

            return files.ToImmutableDictionary();
        }
    }
}