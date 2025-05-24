using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using NTextEditor.Source;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if CSHARP
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NTextEditor.Languages.CSharp;
using CommonCompilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation;
using CommonCompilationOptions = Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions;
using CommonSyntaxHighlightingWalker = NTextEditor.Languages.CSharp.CSharpSyntaxHighlightingWalker;
#elif VISUALBASIC
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NTextEditor.Languages.VisualBasic;
using CommonCompilation = Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation;
using CommonCompilationOptions = Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilationOptions;
using CommonSyntaxHighlightingWalker = NTextEditor.Languages.VisualBasic.VisualBasicSyntaxHighlightingWalker;
#endif

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
                Compilation compilation = CommonCompilation.Create("MyCompilation")
                    .WithOptions(new CommonCompilationOptions(isLibrary ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);

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

        public static IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition, CompilationContainer? _compilation)
        {
            if (_compilation == null)
            {
                // TODO: Maybe throw exception?
                yield break;
            }
            SyntaxNode root = _compilation.CurrentTree.GetRoot();
            var token = root.FindToken(characterPosition, true);
            while (token != default)
            {
                if (token.HasTrailingTrivia)
                {
                    foreach (var span in GetSpansFromTrivia(token.TrailingTrivia).Reverse())
                    {
                        if (span.start < characterPosition)
                        {
                            yield return span;
                        }
                    }
                }
                if (characterPosition > token.SpanStart)
                {
                    yield return (token.SpanStart, token.Span.End);
                }
                if (token.HasLeadingTrivia)
                {
                    foreach (var span in GetSpansFromTrivia(token.LeadingTrivia).Reverse())
                    {
                        if (span.start < characterPosition)
                        {
                            yield return span;
                        }
                    }
                }
                token = token.GetPreviousToken();
            }
        }

        public static IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition, CompilationContainer? _compilation)
        {
            if (_compilation == null)
            {
                // TODO: Maybe throw exception?
                yield break;
            }
            SyntaxNode root = _compilation.CurrentTree.GetRoot();
            var token = root.FindToken(characterPosition, true);
            while (token != default)
            {
                if (token.HasLeadingTrivia
                    && characterPosition < token.SpanStart)
                {
                    foreach (var span in GetSpansFromTrivia(token.LeadingTrivia))
                    {
                        if (span.start >= characterPosition)
                        {
                            yield return span;
                        }
                    }
                }
                if (characterPosition < token.Span.End)
                {
                    yield return (token.SpanStart, token.Span.End);
                }
                if (token.HasTrailingTrivia)
                {
                    foreach (var span in GetSpansFromTrivia(token.TrailingTrivia))
                    {
                        if (span.start >= characterPosition)
                        {
                            yield return span;
                        }
                    }
                }
                token = token.GetNextToken();
            }
        }

        private static IEnumerable<(int start, int end)> GetSpansFromTrivia(SyntaxTriviaList triviaList)
            => GetSpansFromTriviaWhere(triviaList, t => true);

        private static IEnumerable<(int start, int end)> GetSpansFromTriviaWhere(SyntaxTriviaList triviaList, Func<SyntaxTrivia, bool> predicate)
        {
            foreach (var trivia in triviaList.Where(predicate))
            {
                foreach (var span in GetSpansFromTrivia(trivia))
                {
                    yield return span;
                }
            }
        }

        private static IEnumerable<(int start, int end)> GetSpansFromTrivia(SyntaxTrivia trivia)
        {
            string text = trivia.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                // we don't care about whitespace trivia
                yield break;
            }
            int startIndex = trivia.SpanStart;
            bool inWord = !char.IsWhiteSpace(text[0]);
            int wordStart = 0;
            for (int i = 0; i < text.Length; i++)
            {
                bool currentInWord = !char.IsWhiteSpace(text[i]);
                if (inWord && !currentInWord)
                {
                    yield return (wordStart + startIndex, i + startIndex);
                    inWord = false;
                }
                else if (!inWord && currentInWord)
                {
                    wordStart = i;
                    inWord = true;
                }
            }
            if (inWord)
            {
                yield return (wordStart + startIndex, text.Length + startIndex);
            }
        }

        internal static SyntaxHighlightingCollection GetHighlightings(CompilationContainer compilation, SyntaxPalette palette)
        {
            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            IReadOnlyList<int> cumulativeLineLengths = compilation.CumulativeLineLengths;

            foreach (var trivium in compilation.CurrentTree.GetRoot().DescendantTrivia())
            {
                // comments don't get visited by the syntax walker
                if (trivium.IsCommentTrivia())
                {
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting, cumulativeLineLengths);
                }
                else if (trivium.IsKind(SyntaxKind.DisabledTextTrivia)
                    || trivium.IsDirective)
                {
                    AddSpanToHighlighting(trivium.Span, palette.DirectiveColour, highlighting, cumulativeLineLengths);
                }
            }
            var task = compilation.GetDiagnostics();
            CommonSyntaxHighlightingWalker highlighter = new CommonSyntaxHighlightingWalker(compilation.SemanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
                palette);
            highlighter.Visit(compilation.CurrentTree.GetRoot());

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), task.Result, blockLines);
        }

        internal static void HighlightExpressionSyntax(ExpressionSyntax node, SemanticModel semanticModel, SyntaxPalette palette, Action<TextSpan, Color> highlightAction, bool isAttribute = false)
        {
            ISymbol? symbol = semanticModel.GetSymbolInfo(node).Symbol;
            string? identifierText = (node as IdentifierNameSyntax)?.Identifier.Text;
            if (symbol != null)
            {
                if (symbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    // ignore this case - the element type will be picked up later
                }
                else if (symbol is ITypeSymbol typeSymbol)
                {
                    if (identifierText == "var")
                    {
                        highlightAction(node.Span, palette.BlueKeywordColour);
                    }
                    else if (node is TypeSyntax t)
                    {
                        HighlightTypeSyntax(t, semanticModel, palette, highlightAction);
                    }
                }
                else if (symbol is IMethodSymbol methodSymbol)
                {
                    highlightAction(node.Span, isAttribute ? palette.TypeColour : palette.MethodColour);
                }
                else if (symbol is IParameterSymbol || symbol is ILocalSymbol)
                {
                    if (identifierText == "value")
                    {
                        highlightAction(node.Span, palette.BlueKeywordColour);
                    }
                    else
                    {
                        highlightAction(node.Span, palette.LocalVariableColour);
                    }
                }
            }
            else
            {
                if (identifierText == "nameof")
                {
                    highlightAction(node.Span, palette.BlueKeywordColour);
                }
            }
        }

        private static void HighlightTypeSyntax(TypeSyntax typeSyntax, SemanticModel model, SyntaxPalette palette, Action<TextSpan, Color> highlightAction)
        {
            if (typeSyntax is IdentifierNameSyntax identifierNameSyntax)
            {
                highlightAction(identifierNameSyntax.Span, palette.TypeColour);
            }
            else if (typeSyntax is GenericNameSyntax genericNameSyntax)
            {
                highlightAction(genericNameSyntax.Identifier.Span, palette.TypeColour);
                foreach (TypeSyntax typeArgument in genericNameSyntax.TypeArgumentList.Arguments)
                {
                    HighlightExpressionSyntax(typeArgument, model, palette, highlightAction);
                }
            }
            else if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
            {
                HighlightExpressionSyntax(nullableTypeSyntax.ElementType, model, palette, highlightAction);
            }
            else if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
            {
                HighlightExpressionSyntax(arrayTypeSyntax.ElementType, model, palette, highlightAction);
            }
            else if (typeSyntax is TupleTypeSyntax tupleTypeSyntax)
            {
                foreach (TupleElementSyntax element in tupleTypeSyntax.Elements)
                {
#if CSHARP
                    HighlightExpressionSyntax(element.Type, model, palette, highlightAction);
#endif
                }
            }
            else if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
            {
                HighlightExpressionSyntax(qualifiedNameSyntax.Right, model, palette, highlightAction);
            }
            else if (typeSyntax is PredefinedTypeSyntax p)
            {
                //_highlightAction(p.Span, _palette.BlueKeywordColour);
            }
        }
    }
}
