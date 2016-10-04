using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper.Text
{
    public class ConditionStorageBuilder : IStorageBuilder
    {
        public ImmutableDictionary<TypePair, TypeMap> ExplicitTypeMaps { get; set; }
        public ActionNameConvention Convention { get; set; }

        public ConditionStorageBuilder(IDictionary<TypePair, TypeMap> explicitTypeMaps)
        {
            ExplicitTypeMaps = explicitTypeMaps.ToImmutableDictionary();
            Convention = NameConventionsStorage.Condition;
        }

        public string BuildCode()
        {
            List<string> members = new List<string>();

            IteratePropertyMaps((tm, pm) =>
            {
                string memberCode = CreateMemberCode(pm);

                memberCode = memberCode.RemoveDoubleBraces();

                members.Add(memberCode);
            }
            );

            string code = CodeTemplates.Class(members, Convention.Namespace, Convention.ClassShortName);

            return code;
        }

        public void IteratePropertyMaps(Action<TypeMap, PropertyMap> action)
        {
            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (PropertyMap propertyMap in map.PropertyMaps)
                {
                    if (propertyMap.OriginalCondition != null)
                    {
                        action(map, propertyMap);
                    }
                }
            }
        }

        public void InitStorage(Assembly assembly)
        {
            var type = assembly.GetType(Convention.ClassFullName);

            IteratePropertyMaps((tm, pm) =>
            {
                string id = pm.OriginalCondition.Id;
                var func = pm.OriginalCondition.Delegate;

                var fieldInfo = type.GetField(Convention.GetMemberShortName(id));

                fieldInfo.SetValue(null, func);
            }
            );
        }

        private string CreateMemberCode(PropertyMap propertyMap)
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