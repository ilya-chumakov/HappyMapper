using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
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

            IterateStatements((tm, statement) =>
            {
                string methodCode = CreateMethodInnerCode(statement, tm);

                methodCode = methodCode.RemoveDoubleBraces();

                methods.Add(methodCode);
            }
            );
            string code = CodeHelper.BuildClassCode(methods, Convention.Namespace, Convention.ClassShortName);

            return code;
        }

        public void IterateStatements(Action<TypeMap, OriginalStatement> action)
        {
            foreach (var kvp in ExplicitTypeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;

                foreach (var statement in map.BeforeMapStatements)
                {
                    if (statement != null)
                    {
                        action(map, statement);
                    }
                }
            }
        }

        public void InitStorage(Assembly assembly)
        {
            var type = assembly.GetType(Convention.ClassFullName);

            IterateStatements((tm, statement) =>
            {
                var fieldInfo = type.GetField(Convention.GetMemberShortName(statement.Id));

                fieldInfo.SetValue(null, statement.Delegate);
            }
            );
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