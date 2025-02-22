using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using System.Collections.Immutable;
using System.Reflection;

namespace CSharpTextEditor.CS
{
    internal class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        private class CompilationContainer(CSharpCompilation compilation, SyntaxTree previousTree, SemanticModel semanticModel)
        {
            public CSharpCompilation Compilation { get; } = compilation;
            public SyntaxTree PreviousTree { get; } = previousTree;
            public SemanticModel SemanticModel { get; } = semanticModel;

            public static CompilationContainer FromTree(SyntaxTree tree, MetadataReference[] references)
            {
                CSharpCompilation compilation = CSharpCompilation.Create("MyCompilation")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);
                return new CompilationContainer(
                    compilation,
                    tree,
                    compilation.GetSemanticModel(tree));
            }

            public CompilationContainer WithNewTree(SyntaxTree tree)
            {
                CSharpCompilation newCompilation = Compilation.ReplaceSyntaxTree(PreviousTree, tree);
                return new CompilationContainer(newCompilation,
                    tree,
                    newCompilation.GetSemanticModel(tree));
            }
        }

        private CompilationContainer? _compilation;

        internal CSharpSyntaxHighlighter()
        {
        }

        private MetadataReference[] GetReferences()
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

        private bool AssemblyIsValid(Assembly assembly)
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

        public CodeCompletionSuggestion? GetSuggestionAtPosition(int characterPosition, SyntaxPalette syntaxPalette)
        {
            if (_compilation != null)
            {
                // TODO: This without the try-catch
                try
                {
                    var token = _compilation.PreviousTree.GetRoot().FindToken(characterPosition);
                    if (!token.IsKind(SyntaxKind.EndOfFileToken)
                        && token.Parent != null)
                    {
                        var node = token.Parent;
                        ISymbol? symbol = _compilation.SemanticModel.GetDeclaredSymbol(node);
                        bool isDeclaration = false;
                        if (symbol == null)
                        {
                            symbol = SymbolVisitor.FindSymbolWithName(node.ToString(), _compilation.SemanticModel);
                        }
                        else
                        {
                            isDeclaration = node.IsDeclaration();
                        }
                        if (symbol != null)
                        {
                            return SymbolToSuggestion(symbol, syntaxPalette, isDeclaration);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return null;
        }

        public IEnumerable<CodeCompletionSuggestion> GetCodeCompletionSuggestions(string textLine, int position, SyntaxPalette syntaxPalette)
        {
            if (_compilation?.SemanticModel == null)
            {
                return [];
            }
            if (textLine.EndsWith("."))
            {
                textLine = textLine.Substring(0, textLine.Length - 1).Trim();
            }
            string identifierName = textLine.Split(' ', '.', ';', '(').Last();
            ISymbol? symbol = SymbolVisitor.FindSymbolWithName(identifierName, _compilation.SemanticModel);

            IEnumerable<ISymbol> foundSymbols;
            if (CanGetTypeSymbolFromSymbol(symbol, out var namespaceOrTypeSymbol, out bool isInstance))
            {
                foundSymbols = _compilation.SemanticModel.LookupSymbols(position, namespaceOrTypeSymbol, null, true);
                if (isInstance)
                {
                    foundSymbols = foundSymbols.Where(x => !x.IsStatic);
                }
            }
            else
            {
                foundSymbols = _compilation.SemanticModel.LookupSymbols(position, null, null, true);
            }
            return foundSymbols.Select(x => SymbolToSuggestion(x, syntaxPalette));
        }

        private bool CanGetTypeSymbolFromSymbol(ISymbol? symbol, out INamespaceOrTypeSymbol? namespaceOrTypeSymbol, out bool isInstance)
        {
            if (symbol == null)
            {
                namespaceOrTypeSymbol = null;
                isInstance = false;
                return false;
            }
            (namespaceOrTypeSymbol, isInstance) = symbol switch
            {
                INamespaceSymbol namespaceSymbol => ((INamespaceOrTypeSymbol, bool))(namespaceSymbol, false),
                ITypeSymbol typeSymbol => (typeSymbol, false),
                ILocalSymbol localSymbol => (localSymbol.Type, true),
                IParameterSymbol parameterSymbol => (parameterSymbol.Type, true),
                IFieldSymbol fieldSymbol => (fieldSymbol.Type, true),
                IPropertySymbol propertySymbol => (propertySymbol.Type, true),
                _ => (null, false)
            };
            return namespaceOrTypeSymbol != null;
        }

        private CodeCompletionSuggestion SymbolToSuggestion(ISymbol symbol, SyntaxPalette syntaxPalette, bool isDeclaration = false)
        {
            string name = symbol.Name;
            SymbolType type = SymbolType.None;
            HighlightedToolTipBuilder builder = new HighlightedToolTipBuilder(syntaxPalette);
            if (symbol is IMethodSymbol ms)
            {
                type = SymbolType.Method;
                builder.AddType(ms.ReturnType).AddDefault(" ");
                builder.AddType(ms.ContainingType).Add($".{ms.Name}", syntaxPalette.MethodColour).AddDefault("(");
                bool isFirst = true;
                int parameterIndex = 0;
                foreach (IParameterSymbol parameter in ms.Parameters)
                {
                    if (!isFirst)
                    {
                        builder.AddDefault(", ");
                    }
                    isFirst = false;
                    builder.AddType(parameter.Type, parameterIndex).Add($" {parameter.Name}", syntaxPalette.LocalVariableColour, parameterIndex);
                    parameterIndex++;
                }
                builder.AddDefault(")");
                
            }
            else if (symbol is IPropertySymbol ps)
            {
                type = SymbolType.Property;
                builder.AddType(ps.Type);
                builder.AddDefault(" ");
                builder.AddType(ps.ContainingType);
                builder.AddDefault($".{ps.Name} " + "{ ");
                if (ps.GetMethod != null)
                {
                    builder.Add("get", syntaxPalette.BlueKeywordColour);
                    builder.AddDefault("; ");
                }
                if (ps.SetMethod != null)
                {
                    builder.Add("set", syntaxPalette.BlueKeywordColour);
                    builder.AddDefault("; ");
                }
                builder.AddDefault("}");
            }
            else if (symbol is INamedTypeSymbol t)
            {
                if (t.TypeArguments.Length > 0)
                {
                    name += "<>";
                }
                string typeKindName = t.TypeKind.ToString().ToLower();
                builder.Add(typeKindName, syntaxPalette.BlueKeywordColour).AddDefault($" {t.Name}");
                switch (t.TypeKind)
                {
                    case TypeKind.Class:
                        type = SymbolType.Class;
                        break;
                    case TypeKind.Interface:
                        type = SymbolType.Interface;
                        break;
                    case TypeKind.Struct:
                        type = SymbolType.Struct;
                        break;
                }
            }
            else if (symbol is INamespaceSymbol ns)
            {
                type = SymbolType.Namespace;
                builder.Add("namespace", syntaxPalette.BlueKeywordColour).AddDefault($" {ns.ToDisplayString()}");
            }
            else if (symbol is IFieldSymbol f)
            {
                
                if (f.ContainingType?.TypeKind == TypeKind.Enum)
                {
                    builder.AddType(f.ContainingType).AddDefault($"{f.Name} = {f.ConstantValue}");
                    type = SymbolType.EnumMember;
                }
                else
                {
                    string prefix;
                    if (f.IsConst)
                    {
                        prefix = "constant";
                        type = SymbolType.Constant;
                    }
                    else
                    {
                        prefix = "field";
                        type = SymbolType.Field;
                    }
                    builder.AddDefault($"({prefix}) ").AddType(f.Type).AddDefault($" {f.Name}");
                    if (f.HasConstantValue)
                    {
                        builder.AddDefault($" = {f.ConstantValue}");
                    }
                }
            }
            else if (symbol is ILocalSymbol local)
            {
                string prefix = local.IsConst ? "local constant" : "local variable";
                type = SymbolType.Local;
                builder.AddDefault($"({prefix}) ").AddType(local.Type).AddDefault($" {local.Name}");
                if (local.HasConstantValue)
                {
                    builder.AddDefault($" = {local.ConstantValue}");
                }
            }
            else if (symbol is IParameterSymbol p)
            {
                type = SymbolType.Local;
                builder.AddDefault("(parameter) ").AddType(p.Type).AddDefault($" {p.Name}");
            }
            return new CodeCompletionSuggestion(name, type, builder, isDeclaration);
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

        public SyntaxHighlightingCollection GetHighlightings(IEnumerable<string> sourceLines, SyntaxPalette palette)
        {
            List<string> timings = new List<string>();
            (string sourceText, IImmutableList<int> cumulativeLineLengths) = GetText(sourceLines);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);

            if (_compilation == null)
            {
                _compilation = CompilationContainer.FromTree(tree, GetReferences());
            }
            else
            {
                _compilation = _compilation.WithNewTree(tree);
            }

            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();

            foreach (var trivium in tree.GetRoot().DescendantTrivia())
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
            var task = Task.Run(() =>
                {
                    List<(SourceCodePosition start, SourceCodePosition end, string message)> errors = new List<(SourceCodePosition start, SourceCodePosition end, string message)>();
                    foreach (var diagnostic in tree.GetDiagnostics().Concat(_compilation.Compilation.GetDiagnostics()))
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.Start, cumulativeLineLengths);
                            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.End, cumulativeLineLengths);
                            errors.Add((start, end, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                        }
                    }
                    return errors;
                });
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(_compilation.SemanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
                palette);
            highlighter.Visit(tree.GetRoot());

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), task.Result, blockLines);
        }

        private IEnumerable<(int start, int end)> GetSpansFromTrivia(SyntaxTriviaList triviaList)
            => GetSpansFromTriviaWhere(triviaList, t => true);

        private IEnumerable<(int start, int end)> GetSpansFromTriviaWhere(SyntaxTriviaList triviaList, Func<SyntaxTrivia, bool> predicate)
        {
            foreach (var trivia in triviaList.Where(predicate))
            {
                foreach (var span in GetSpansFromTrivia(trivia))
                {
                    yield return span;
                }
            }
        }

        private IEnumerable<(int start, int end)> GetSpansFromTrivia(SyntaxTrivia trivia)
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

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
        {
            if (_compilation == null)
            {
                // TODO: Maybe throw exception?
                yield break;
            }
            SyntaxNode root = _compilation.PreviousTree.GetRoot();
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

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
        {
            if (_compilation == null)
            {
                // TODO: Maybe throw exception?
                yield break;
            }
            SyntaxNode root = _compilation.PreviousTree.GetRoot();
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

        public IEnumerable<(int start, int end)> GetSpansFromTextLine(string textLine)
        {
            foreach (SyntaxToken token in CSharpSyntaxTree.ParseText(textLine).GetRoot().DescendantTokens())
            {
                if (token.HasLeadingTrivia)
                {
                    foreach (var span in GetSpansFromTrivia(token.LeadingTrivia))
                    {
                        yield return span;
                    }
                }
                yield return (token.Span.Start, token.Span.End);
                if (token.HasTrailingTrivia)
                {
                    foreach (var span in GetSpansFromTrivia(token.TrailingTrivia))
                    {
                        yield return span;
                    }
                }
            }
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting, IReadOnlyList<int> cumulativeLineLengths)
        {
            if (span.IsEmpty)
            {
                return;
            }
            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths);
            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths);
            highlighting.Add(new SyntaxHighlighting(start, end, colour));
        }
    }
}
