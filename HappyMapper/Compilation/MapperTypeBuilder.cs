using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper.Extended.Net4.SharedTools;
using HappyMapper.AutoMapper.ConfigurationAPI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace HappyMapper.Compilation
{
    internal static class MapperTypeBuilder
    {
        public static CSharpCompilation CreateCompilation(string[] texts, HashSet<Type> types)
        {
            var trees = CreateSyntaxTrees(texts);

            var metadataReferences = GetMetadataReferences(types);

            return CreateCompilation(trees, metadataReferences);
        }

        public static CSharpCompilation CreateCompilation(string[] texts, HashSet<string> locations)
        {
            var trees = CreateSyntaxTrees(texts);

            var metadataReferences = GetMetadataReferences(locations);

            return CreateCompilation(trees, metadataReferences);
        }

        private static SyntaxTree[] CreateSyntaxTrees(string[] texts)
        {
            var trees = new SyntaxTree[texts.Length];

            for (int index = 0; index < texts.Length; index++)
            {
                trees[index] = CSharpSyntaxTree.ParseText(texts[index]);
            }
            return trees;
        }

        private static IReadOnlyList<MetadataReference> GetMetadataReferences(HashSet<Type> types)
        {
            var locations = types.Select(t => t.Assembly.Location).ToList();

            return GetMetadataReferences(locations);
        }

        private static IReadOnlyList<MetadataReference> GetMetadataReferences(HashSet<string> locations)
        {
            return GetMetadataReferences(locations.ToList());
        }

        private static IReadOnlyList<MetadataReference> GetMetadataReferences(IReadOnlyList<string> locations)
        {
            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(HappyMapperException).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(CollectionExtensions).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(ResolutionContext).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)); //only for CollectionBuilder
            references.Add(MetadataReference.CreateFromFile(typeof(Queryable).Assembly.Location)); //only for CollectionBuilder

            references.AddRange(locations.Select(location => MetadataReference.CreateFromFile(location)));

            return references;
        }

        public static CSharpCompilation CreateCompilation(
            SyntaxTree[] syntaxTrees, 
            IReadOnlyList<MetadataReference> references)
        {
            string assemblyName = Path.GetRandomFileName();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            return compilation;
        }

        public static Assembly CreateAssembly(CSharpCompilation compilation)
        {
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    return assembly;
                }
            }
            throw new HappyMapperException("Can't compile the mappers!");
        }
    }
}