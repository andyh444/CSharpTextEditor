using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using NTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.Common
{
    internal class CodeAnalysisHelper
    {
        internal class CompilationContainer(Compilation compilation, SyntaxTree previousTree, SemanticModel semanticModel, IReadOnlyList<int> cumulativeLineLengths)
        {
            public Compilation Compilation { get; } = compilation;
            public SyntaxTree CurrentTree { get; } = previousTree;
            public SemanticModel SemanticModel { get; } = semanticModel;
            public IReadOnlyList<int> CumulativeLineLengths { get; } = cumulativeLineLengths;

            public static CompilationContainer FromTree(SyntaxTree tree, MetadataReference[] references, IReadOnlyList<int> cumulativeLineLengths, bool isLibrary)
            {
#if CSHARP
                Compilation compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("MyCompilation")
                    .WithOptions(new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(isLibrary ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);
#elif VISUALBASIC
                Compilation compilation = Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.Create("MyCompilation")
                    .WithOptions(new Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilationOptions(isLibrary ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);
#endif
                return new CompilationContainer(
                    compilation,
                    tree,
                    compilation.GetSemanticModel(tree),
                    cumulativeLineLengths);
            }

            public CompilationContainer WithNewTree(SyntaxTree tree, IReadOnlyList<int> cumulativeLineLengths)
            {
                Compilation newCompilation = Compilation.ReplaceSyntaxTree(CurrentTree, tree);
                return new CompilationContainer(newCompilation,
                    tree,
                    newCompilation.GetSemanticModel(tree),
                    cumulativeLineLengths);
            }

            public Task<IReadOnlyList<SyntaxDiagnostic>> GetDiagnostics()
            {
                return Task.Run<IReadOnlyList<SyntaxDiagnostic>>(() =>
                {
                    List<SyntaxDiagnostic> errors = new List<SyntaxDiagnostic>();
                    foreach (var diagnostic in Compilation.GetDiagnostics())
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.Start, CumulativeLineLengths);
                            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.End, CumulativeLineLengths);
                            errors.Add(new SyntaxDiagnostic(start, end, diagnostic.Id, diagnostic.GetMessage()));
                        }
                    }
                    return errors;
                });
            }
        }

        internal static (string text, ImmutableList<int> cumulativeLineLengths) GetText(IEnumerable<string> lines)
        {
            ImmutableList<int>.Builder builder = ImmutableList.CreateBuilder<int>();
            int previous = 0;
            int newLineLength = Environment.NewLine.Length;
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string line in lines)
            {
                if (!first)
                {
                    sb.AppendLine();
                }
                first = false;
                sb.Append(line);
                int cumulativeSum = previous + line.Length + newLineLength;
                builder.Add(cumulativeSum);
                previous = cumulativeSum;
            }
            return (sb.ToString(), builder.ToImmutable());
        }

        internal static MetadataReference[] GetReferences()
        {
            /*var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
            };*/
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(AssemblyIsValid)
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .ToArray();
        }

        private static bool AssemblyIsValid(Assembly assembly)
        {
            try
            {
                return !string.IsNullOrEmpty(assembly.Location);
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting, IReadOnlyList<int> cumulativeLineLengths)
        {
            if (span.IsEmpty)
            {
                return;
            }
            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths);
            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths);
            highlighting.Add(new SyntaxHighlighting(start, end, colour));
        }

        internal static void Execute(CompilationContainer? compilation, TextWriter output)
        {
            try
            {
                if (compilation == null)
                {
                    throw new CSharpTextEditorException();
                }
                using MemoryStream ms = new MemoryStream();
                EmitResult result = compilation.Compilation.Emit(ms);
                if (!result.Success)
                {
                    output.WriteLine("Compilation failed");
                    foreach (Diagnostic diagnostic in result.Diagnostics)
                    {
                        output.WriteLine(diagnostic);
                    }
                    return;
                }
                ms.Position = 0;
                Assembly assembly = Assembly.Load(ms.ToArray());
                MethodInfo? entryPoint = assembly.EntryPoint;
                if (entryPoint == null)
                {
                    output.WriteLine("No entry point found");
                    return;
                }
                int parameterCount = entryPoint.GetParameters().Length;
                if (parameterCount == 1)
                {
                    entryPoint.Invoke(null, [new string[0]]);
                }
                else if (parameterCount == 0)
                {
                    entryPoint.Invoke(null, Array.Empty<object>());
                }
                else
                {
                    throw new CSharpTextEditorException("Unexpected parameter count");
                }
            }
            catch (Exception ex)
            {
                output.WriteLine("Unhandled exception executing code:");
                output.WriteLine(ex.Message);
            }
        }
    }
}
