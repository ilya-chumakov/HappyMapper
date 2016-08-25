using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace OrdinaryMapper
{
    public static class MapperTypeBuilder
    {
        public static Type CreateMapperType(string text, MapContext context)
        {
            var compilation = CreateCompilation(new[] { text }, context.ToHashSet());

            var assembly = CreateAssembly(compilation);

            return assembly.GetType($"{context.MapperClassFullName}");
        }

        public static CSharpCompilation CreateCompilation(string[] texts, HashSet<Type> types)
        {
            var trees = new SyntaxTree[texts.Length];

            for (int index = 0; index < texts.Length; index++)
            {
                trees[index] = CSharpSyntaxTree.ParseText(texts[index]);
            }

            return CreateCompilation(trees, types);
        }

        public static CSharpCompilation CreateCompilation(SyntaxTree[] syntaxTrees, HashSet<Type> types)
        {
            string assemblyName = Path.GetRandomFileName();

            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(OrdinaryMapperException).Assembly.Location));

            foreach (Type type in types)
            {
                references.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
            }

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
            return null;
        }
    }
}