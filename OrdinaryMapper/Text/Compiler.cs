using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace OrdinaryMapper
{
    public class Compiler
    {
        public static Dictionary<TypePair, object> CompileToAssembly(MapperConfigurationExpression config, IDictionary<TypePair, TypeMap> typeMaps)
        {
            var textBuilder = new MapperTextBuilderV2(typeMaps, config);

            var files = textBuilder.CreateCodeFiles();


            string[] trees = files.Values.Select(x => x.Code).ToArray();
            HashSet<string> locations = textBuilder.DetectedLocations;

            CSharpCompilation compilation = MapperTypeBuilder.CreateCompilation(trees, locations);

            Assembly assembly = MapperTypeBuilder.CreateAssembly(compilation);

            Dictionary<TypePair, object> delegateCache = new Dictionary<TypePair, object>();

            foreach (var kvp in typeMaps)
            {
                TypePair typePair = kvp.Key;
                TypeMap map = kvp.Value;
                CodeFile codeFile = files[typePair];

                var type = assembly.GetType(codeFile.ClassFullName);

                var @delegate = Delegate.CreateDelegate(map.MapDelegateType, type, codeFile.MapperMethodName);

                delegateCache.Add(typePair, @delegate);
            }

            return delegateCache;
        }
    }
}