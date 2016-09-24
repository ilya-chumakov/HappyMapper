using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class ConditionStorageBuilder : IStorageBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }
        public ActionNameConvention Convention { get; set; }

        public ConditionStorageBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            Convention = NameConventions.Condition;
        }

        public string BuildCode()
        {
            List<string> methods = new List<string>();

            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (PropertyMap propertyMap in map.PropertyMaps)
                {
                    if (propertyMap.OriginalCondition != null)
                    {
                        string methodCode = CreateMethodInnerCode(propertyMap);

                        methodCode = methodCode.RemoveDoubleBraces();

                        methods.Add(methodCode);
                    }
                }
            }

            string code = CodeHelper.BuildClassCode(methods, Convention.Namespace, Convention.ClassShortName);

            return code;
        }

        public void InitStorage(IDictionary<TypePair, TypeMap> typeMaps, Assembly assembly)
        {
            var type = assembly.GetType(Convention.ClassFullName);

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (PropertyMap propertyMap in map.PropertyMaps)
                {
                    if (propertyMap.OriginalCondition != null)
                    {
                        string id = propertyMap.OriginalCondition.Id;
                        var func = propertyMap.OriginalCondition.Delegate;

                        var fieldInfo = type.GetField(Convention.GetMemberShortName(id));

                        fieldInfo.SetValue(null, func);
                    }
                }
            }
        }

        private string CreateMethodInnerCode(PropertyMap propertyMap)
        {
            string id = propertyMap.OriginalCondition.Id;
            string srcTypeName = propertyMap.TypeMap.SourceType.FullName.NormalizeTypeName();
            string destTypeName = propertyMap.TypeMap.DestinationType.FullName.NormalizeTypeName();

            string type = $"Func<{srcTypeName}, {destTypeName}, bool>";

            var builder = new StringBuilder();

            builder.AppendLine($"public static {type} {Convention.MemberPrefix}{id};");

            return builder.ToString();
        }
    }
}