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
            var compilation = CreateCompilation(text, context);

            var assembly = CreateAssembly(compilation);

            return assembly.GetType($"{context.MapperClassFullName}");
        }

        private static CSharpCompilation CreateCompilation(string text, MapContext context)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text);

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = {
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                MetadataReference.CreateFromFile(context.SrcType.Assembly.Location),
                MetadataReference.CreateFromFile(context.DestType.Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OrdinaryMapperException).Assembly.Location),
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
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