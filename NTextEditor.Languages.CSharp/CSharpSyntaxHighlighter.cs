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
using NTextEditor.Source;
using NTextEditor.View;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using NTextEditor.Languages;
using NTextEditor;

namespace NTextEditor.Languages.CSharp
{
    internal class CSharpSyntaxHighlighter : ISyntaxHighlighter, ICodeExecutor
    {
        private class CompilationContainer(CSharpCompilation compilation, SyntaxTree previousTree, SemanticModel semanticModel, IReadOnlyList<int> cumulativeLineLengths)
        {
            public CSharpCompilation Compilation { get; } = compilation;
            public SyntaxTree CurrentTree { get; } = previousTree;
            public SemanticModel SemanticModel { get; } = semanticModel;
            public IReadOnlyList<int> CumulativeLineLengths { get; } = cumulativeLineLengths;

            public static CompilationContainer FromTree(SyntaxTree tree, MetadataReference[] references, IReadOnlyList<int> cumulativeLineLengths, bool isLibrary)
            {
                CSharpCompilation compilation = CSharpCompilation.Create("MyCompilation")
                    .WithOptions(new CSharpCompilationOptions(isLibrary ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication))
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
                CSharpCompilation newCompilation = Compilation.ReplaceSyntaxTree(CurrentTree, tree);
                return new CompilationContainer(newCompilation,
                    tree,
                    newCompilation.GetSemanticModel(tree),
                    cumulativeLineLengths);
            }
        }

        private CompilationContainer? _compilation;
        private readonly bool isLibrary;

        internal CSharpSyntaxHighlighter(bool isLibrary = true)
        {
            this.isLibrary = isLibrary;
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

        public CodeCompletionSuggestion? GetSymbolInfoAtPosition(int characterPosition, SyntaxPalette palette)
        {
            if (_compilation == null)
            {
                return null;
            }
            SyntaxToken token = _compilation.CurrentTree.GetRoot().FindToken(Math.Max(0, characterPosition - 1));
            ISymbol? symbol = GetSymbol(token, _compilation, out string? name, out bool isConstructor, out _);
            if (symbol != null)
            {
                return SymbolToSuggestion(symbol, palette);
            }
            return null;
        }

        public IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette syntaxPalette, out int argumentIndex)
        {
            // used by method completion/hover tooltip
            argumentIndex = -1;
            if (_compilation == null)
            {
                return [];
            }
            SyntaxToken token = _compilation.CurrentTree.GetRoot().FindToken(Math.Max(0, characterPosition - 1));

            IEnumerable<ISymbol> foundSymbols = [];

            ISymbol? symbol = GetSymbol(token, _compilation, out string? name, out bool isConstructor, out argumentIndex);
            if (symbol != null
                && CanGetTypeSymbolFromSymbol(symbol, out var namespaceOrTypeSymbol, out bool isInstance))
            {
                if (isConstructor
                    && namespaceOrTypeSymbol is ITypeSymbol ts)
                {
                    foundSymbols = ts.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Constructor);
                }
                else
                {
                    IEnumerable<ISymbol> typeSymbols = _compilation.SemanticModel.LookupSymbols(characterPosition, namespaceOrTypeSymbol, name, true);
                    if (isInstance)
                    {
                        typeSymbols = typeSymbols.Where(x => !x.IsStatic && x is not ITypeSymbol);
                    }
                    else if (namespaceOrTypeSymbol is ITypeSymbol)
                    {
                        typeSymbols = typeSymbols.Where(x => x.IsStatic);
                    }
                    foundSymbols = typeSymbols;
                }
            }
            return foundSymbols.Select(x => SymbolToSuggestion(x, syntaxPalette)).ToList();
        }

        private ISymbol? GetSymbol(SyntaxToken sourceToken, CompilationContainer compilation, out string? name, out bool isConstructor, out int argumentIndex)
        {
            // try to extract the most relevant symbol from the token - normally a namespace or type symbol

            isConstructor = false;
            name = null;
            argumentIndex = -1;

            SyntaxNode? currentNode = sourceToken.Parent;

            while (currentNode != null)
            {
                switch (currentNode)
                {
                    case MemberAccessExpressionSyntax maes:
                        currentNode = maes.Expression is MemberAccessExpressionSyntax maes2
                            ? maes2.Name
                            : (SyntaxNode)maes.Expression;
                        break;
                    case ArgumentListSyntax als:
                        if (sourceToken.IsKind(SyntaxKind.CommaToken))
                        {
                            argumentIndex = 1 + als.Arguments.GetSeparators().ToList().IndexOf(sourceToken);
                        }
                        else if (sourceToken.IsKind(SyntaxKind.OpenParenToken))
                        {
                            argumentIndex = 0;
                        }
                        currentNode = als.Parent;
                        break;
                    case InvocationExpressionSyntax ies:
                        name = (ies.Expression as MemberAccessExpressionSyntax)?.Name.ToString()
                                ?? (ies.Expression as IdentifierNameSyntax)?.ToString();
                        currentNode = ies.Expression;
                        break;
                    case ObjectCreationExpressionSyntax oces:
                        isConstructor = true;
                        currentNode = oces.Type;
                        break;
                    case QualifiedNameSyntax qns:
                        currentNode = qns.Right.SpanStart < sourceToken.SpanStart
                            ? qns.Right
                            : (SyntaxNode)qns.Left;
                        break;

                    case ParameterSyntax ps:
                        return SymbolVisitor.FindSymbolsWithName(ps.Identifier.Text, compilation.SemanticModel).FirstOrDefault();
                    case VariableDeclaratorSyntax vds:
                        return SymbolVisitor.FindSymbolsWithName(vds.Identifier.Text, compilation.SemanticModel).FirstOrDefault();
                    case MethodDeclarationSyntax mds:
                        return SymbolVisitor.FindSymbolsWithName(mds.Identifier.Text, compilation.SemanticModel).FirstOrDefault();
                    case PropertyDeclarationSyntax pds:
                        return SymbolVisitor.FindSymbolsWithName(pds.Identifier.Text, compilation.SemanticModel).FirstOrDefault(x => x is IPropertySymbol);
                    default:
                        return compilation.SemanticModel.GetSymbolInfo(currentNode).Symbol
                            ?? SymbolVisitor.FindSymbolsWithName(currentNode.ToString(), compilation.SemanticModel).FirstOrDefault();
                }
            }
            return null;
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
                IMethodSymbol methodSymbol => (methodSymbol.ContainingType, true),
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
                if (ms.MethodKind == MethodKind.Constructor)
                {
                    builder.AddType(ms.ContainingType);
                }
                else
                {
                    builder.AddType(ms.ReturnType).AddDefault(" ");
                    builder.AddType(ms.ContainingType).Add($".{ms.Name}", syntaxPalette.MethodColour);
                }
                builder.AddDefault("(");
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
                builder.Add(typeKindName, syntaxPalette.BlueKeywordColour).AddDefault(" ").AddType(t, fullyQualified: true);
                //.AddDefault($" {t.ToDisplayString()}");
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

        public void Update(IEnumerable<string> sourceLines)
        {
            (string sourceText, IImmutableList<int> cumulativeLineLengths) = GetText(sourceLines);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);

            if (_compilation == null)
            {
                _compilation = CompilationContainer.FromTree(tree, GetReferences(), cumulativeLineLengths, isLibrary);
            }
            else
            {
                _compilation = _compilation.WithNewTree(tree, cumulativeLineLengths);
            }
        }

        public SyntaxHighlightingCollection GetHighlightings(SyntaxPalette palette)
        {
            if (_compilation == null)
            {
                throw new CSharpTextEditorException("Must call Update before calling GetHighlightings");
            }
            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            IReadOnlyList<int> cumulativeLineLengths = _compilation.CumulativeLineLengths;

            foreach (var trivium in _compilation.CurrentTree.GetRoot().DescendantTrivia())
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
                    List<SyntaxDiagnostic> errors = new List<SyntaxDiagnostic>();
                    foreach (var diagnostic in _compilation.Compilation.GetDiagnostics())
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.Start, cumulativeLineLengths);
                            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.End, cumulativeLineLengths);
                            errors.Add(new SyntaxDiagnostic(start, end, diagnostic.Id, diagnostic.GetMessage()));
                        }
                    }
                    return errors;
                });
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(_compilation.SemanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
                palette);
            highlighter.Visit(_compilation.CurrentTree.GetRoot());

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

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
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

        public void Execute(TextWriter output)
        {
            try
            {
                if (_compilation == null)
                {
                    throw new CSharpTextEditorException();
                }
                using MemoryStream ms = new MemoryStream();
                EmitResult result = _compilation.Compilation.Emit(ms);
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
                entryPoint.Invoke(null, [new string[0]]);
            }
            catch (Exception ex)
            {
                output.WriteLine("Unhandled exception executing code:");
                output.WriteLine(ex.Message);
            }
        }
    }
}
