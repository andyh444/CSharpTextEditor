using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.Serialization;
using System.Runtime;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        class SymbolVisitor : CSharpSyntaxWalker
        {
            private readonly string _symbolName;
            private readonly SemanticModel _semanticModel;

            public ISymbol FoundSymbol { get; private set; }

            public SymbolVisitor(string symbolName, SemanticModel semanticModel)
            {
                _symbolName = symbolName;
                _semanticModel = semanticModel;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (FoundSymbol == null)
                {
                    // Check if the identifier name matches the symbol name
                    if (_symbolName.EndsWith(node.Identifier.Text))
                    {
                        // Get the symbol information for the identifier
                        FoundSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
                    }
                    else
                    {
                        base.VisitIdentifierName(node);
                    }
                }
            }
        }

        private Func<int, SourceCodePosition> _getLineAndColumnNumber;
        private MetadataReference[] references;
        private CSharpCompilation _compilation;
        private SyntaxTree _previousTree;
        private SemanticModel _semanticModel;

        internal CSharpSyntaxHighlighter(Func<int, SourceCodePosition> getLineAndColumnNumber)
        {
            _getLineAndColumnNumber = getLineAndColumnNumber;
            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            /*references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
            };*/
            references = AppDomain.CurrentDomain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location)).ToArray();
        }

        public IEnumerable<CodeCompletionSuggestion> GetCodeCompletionSuggestions(string textLine, int position)
        {
            if (_semanticModel != null)
            {
                if (textLine.EndsWith("."))
                {
                    textLine = textLine.Substring(0, textLine.Length - 1).Trim();
                }
                var visitor = new SymbolVisitor(textLine, _semanticModel);
                visitor.Visit(_semanticModel.SyntaxTree.GetRoot());
                ISymbol symbol = visitor.FoundSymbol;
                if (symbol is INamespaceSymbol namespaceSymbol)
                {
                    return _semanticModel.LookupSymbols(position, namespaceSymbol).Select(SymbolToSuggestion);
                }
                else if (symbol is ITypeSymbol typeSymbol)
                {
                    return _semanticModel.LookupSymbols(position, typeSymbol).Where(x => x.IsStatic).Select(SymbolToSuggestion);
                }
                else if (symbol is ILocalSymbol localSymbol)
                {
                    return _semanticModel.LookupSymbols(position, localSymbol.Type).Where(x => !x.IsStatic).Select(SymbolToSuggestion);
                }
            }
            return Enumerable.Empty<CodeCompletionSuggestion>();
        }

        private CodeCompletionSuggestion SymbolToSuggestion(ISymbol symbol)
        {
            string name = symbol.Name;
            SymbolType type = SymbolType.None;
            if (symbol is IMethodSymbol)
            {
                type = SymbolType.Method;
            }
            else if (symbol is IPropertySymbol)
            {
                type = SymbolType.Property;
            }
            return new CodeCompletionSuggestion(name, type);
        }

        private IEnumerable<string> GetSuggestionsFromSymbol(ISymbol symbol)
        {
            if (symbol is ITypeSymbol typeSymbol)
            {
                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member.IsStatic)
                    {
                        yield return member.Name;
                    }
                }
            }
            else if (symbol is INamespaceSymbol namespaceSymbol)
            {
                foreach (var member in namespaceSymbol.GetMembers())
                {
                    yield return member.Name;
                }
            }
            else if (symbol is ILocalSymbol localSymbol)
            {
                foreach (var member in localSymbol.Type.GetMembers())
                {
                    if (!member.IsStatic)
                    {
                        yield return member.Name;
                    }
                }
            }
        }

        public SyntaxHighlightingCollection GetHighlightings(string sourceText, SyntaxPalette palette)
        {
            List<string> timings = new List<string>();
            Stopwatch sw1 = Stopwatch.StartNew();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            sw1.Stop();
            timings.Add($"ParseText took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            
            if (_compilation != null
                && _previousTree != null)
            {
                _compilation = _compilation.ReplaceSyntaxTree(_previousTree, tree);
            }
            else
            {
                _compilation = CSharpCompilation.Create("MyCompilation")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);
            }
            _previousTree = tree;

            sw1.Stop();
            timings.Add($"create compilation took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            _semanticModel = _compilation.GetSemanticModel(tree);
            sw1.Stop();
            timings.Add($"getsemanticmodel took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            

            foreach (var trivium in tree.GetRoot().DescendantTrivia())
            {
                // comments don't get visited by the syntax walker
                if (trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting);
                }
            }
            sw1.Stop();
            timings.Add($"iterate over descendent trivia took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            var task = Task.Run(() =>
                {
                    List<(SourceCodePosition start, SourceCodePosition end, string message)> errors = new List<(SourceCodePosition start, SourceCodePosition end, string message)>();
                    foreach (var diagnostic in tree.GetDiagnostics().Concat(_compilation.GetDiagnostics()))
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            SourceCodePosition start = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.Start);
                            SourceCodePosition end = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.End);
                            errors.Add((start, end, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                        }
                    }
                    return errors;
                });
            sw1.Stop();
            timings.Add($"iterate over errors took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(_semanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting),
                (span) => blockLines.Add((_getLineAndColumnNumber(span.Start).LineNumber, _getLineAndColumnNumber(span.End).LineNumber)),
                palette);
            highlighter.Visit(tree.GetRoot());
            sw1.Stop();
            timings.Add($"walking syntax took {sw1.Elapsed.TotalMilliseconds} ms");

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), task.Result, blockLines);
        }

        public IEnumerable<(int start, int end)> GetSpansFromTextLine(string textLine)
        {
            foreach (SyntaxToken token in CSharpSyntaxTree.ParseText(textLine).GetRoot().DescendantTokens())
            {
                yield return (token.Span.Start, token.Span.End);
            }
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting)
        {
            SourceCodePosition start = _getLineAndColumnNumber(span.Start);
            SourceCodePosition end = _getLineAndColumnNumber(span.End);
            highlighting.Add(new SyntaxHighlighting(start, end, colour));
        }
    }
}
