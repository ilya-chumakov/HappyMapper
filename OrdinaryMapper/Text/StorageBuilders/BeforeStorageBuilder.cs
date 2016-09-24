using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace OrdinaryMapper
{
    public class BeforeStorageBuilder : IStorageBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }
        public ActionNameConvention Convention { get; set; }

        public BeforeStorageBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            Convention = NameConventions.BeforeMap;
        }

        public string BuildCode()
        {
            List<string> methods = new List<string>();

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (var action in map.BeforeMapStatements)
                {
                    if (action != null)
                    {
                        string methodCode = CreateMethodInnerCode(action, map);

                        methodCode = methodCode.RemoveDoubleBraces();

                        methods.Add(methodCode);
                    }
                }
            }

            string code = CodeHelper.BuildClassCode(methods, Convention.Namespace, Convention.ClassShortName);

            return code;
        }

        private string CreateMethodInnerCode(OriginalStatement statement, TypeMap map)
        {
            string id = statement.Id;
            string srcTypeName = map.SourceType.FullName.NormalizeTypeName();
            string destTypeName = map.DestinationType.FullName.NormalizeTypeName();
            string contextTypeName = typeof (ResolutionContext).FullName;

            string type = $"Action<{srcTypeName}, {destTypeName}, {contextTypeName}>";

            var builder = new StringBuilder();
            
            builder.AppendLine($"public static {type} {Convention.MemberPrefix}{id};");

            return builder.ToString();
        }
    }
}