using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace OrdinaryMapper.Tests.Roslyn
{
    public class ManySyntaxTrees_Tests
    {
        [Test]
        public void MyMethod()
        {
            string classA = "namespace X { class A {} }";
            string classB = "namespace X { class B {} }";

            SyntaxTree treeA = CSharpSyntaxTree.ParseText(classA);
            SyntaxTree treeB = CSharpSyntaxTree.ParseText(classB);

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = {
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { treeA, treeB },
                references: references,
                options: new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            var assembly = MapperTypeBuilder.CreateAssembly(compilation);

            Assert.IsNotNull(assembly);
        } 
    }
}