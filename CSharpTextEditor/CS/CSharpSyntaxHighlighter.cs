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
                    if (_symbolName == node.Identifier.Text)
                    {
                        var symbolInfo = _semanticModel.GetSymbolInfo(node);
                        if (symbolInfo.Symbol != null)
                        {
                            FoundSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
                        }
                        else if (symbolInfo.CandidateSymbols.Length == 1)
                        {
                            FoundSymbol = symbolInfo.CandidateSymbols[0];
                        }
                    }
                    else
                    {
                        base.VisitIdentifierName(node);
                    }
                }
            }
        }

        private CSharpCompilation _compilation;
        private SyntaxTree _previousTree;
        private SemanticModel _semanticModel;

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

        public CodeCompletionSuggestion GetSuggestionAtPosition(int characterPosition, SyntaxPalette syntaxPalette)
        {
            // TODO: This without the try-catch
            try
            {
                var token = _previousTree.GetRoot().FindToken(characterPosition);
                if (!token.IsKind(SyntaxKind.EndOfFileToken))
                {
                    var node = token.Parent;
                    ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
                    bool isDeclaration = false;
                    if (symbol == null)
                    {
                        var visitor = new SymbolVisitor(node.ToString(), _semanticModel);
                        visitor.Visit(_semanticModel.SyntaxTree.GetRoot());
                        symbol = visitor.FoundSymbol;
                    }
                    else
                    {
                        isDeclaration = IsDeclaration(node);
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
            return null;
        }

        private bool IsDeclaration(SyntaxNode node)
        {
            // dirty hack
            return node.GetType().ToString().Contains("DeclarationSyntax");
        }

        public IEnumerable<CodeCompletionSuggestion> GetCodeCompletionSuggestions(string textLine, int position, SyntaxPalette syntaxPalette)
        {
            if (_semanticModel != null)
            {
                if (textLine.EndsWith("."))
                {
                    textLine = textLine.Substring(0, textLine.Length - 1).Trim();
                }
                var visitor = new SymbolVisitor(textLine.Split(' ', '.', ';', '(').Last(), _semanticModel);
                visitor.Visit(_semanticModel.SyntaxTree.GetRoot());
                ISymbol symbol = visitor.FoundSymbol;
                if (symbol is INamespaceSymbol namespaceSymbol)
                {
                    return _semanticModel.LookupSymbols(position, namespaceSymbol, null, true).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else if (symbol is ITypeSymbol typeSymbol)
                {
                    return _semanticModel.LookupSymbols(position, typeSymbol, null, true).Where(x => x.IsStatic).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else if (symbol is ILocalSymbol localSymbol)
                {
                    return _semanticModel.LookupSymbols(position, localSymbol.Type, null, true).Where(x => !x.IsStatic).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else if (symbol is IParameterSymbol parameterSymbol)
                {
                    return _semanticModel.LookupSymbols(position, parameterSymbol.Type, null, true).Where(x => !x.IsStatic).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else if (symbol is IFieldSymbol fieldSymbol)
                {
                    return _semanticModel.LookupSymbols(position, fieldSymbol.Type, null, true).Where(x => !x.IsStatic).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else if (symbol is IPropertySymbol propertySymbol)
                {
                    return _semanticModel.LookupSymbols(position, propertySymbol.Type, null, true).Where(x => !x.IsStatic).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
                else
                {
                    return _semanticModel.LookupSymbols(position, null, null, true).Select(x => SymbolToSuggestion(x, syntaxPalette));
                }
            }
            return Enumerable.Empty<CodeCompletionSuggestion>();
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
            Stopwatch sw1 = Stopwatch.StartNew();
            (string sourceText, IImmutableList<int> cumulativeLineLengths) = GetText(sourceLines);
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
                    .AddReferences(GetReferences())
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
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                    || trivium.IsKind(SyntaxKind.EndOfDocumentationCommentToken)
                    || trivium.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                    || trivium.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting, cumulativeLineLengths);
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
                            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.Start, cumulativeLineLengths);
                            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(diagnostic.Location.SourceSpan.End, cumulativeLineLengths);
                            errors.Add((start, end, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                        }
                    }
                    return errors;
                });
            sw1.Stop();
            timings.Add($"iterate over errors took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(_semanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
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

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
        {
            SyntaxNode root = _previousTree.GetRoot();
            var token = root.FindToken(characterPosition, true);
            while (token != default)
            {
                yield return (token.SpanStart, token.Span.End);
                token = token.GetPreviousToken();
            }
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting, IReadOnlyList<int> cumulativeLineLengths)
        {
            SourceCodePosition start = SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths);
            SourceCodePosition end = SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths);
            highlighting.Add(new SyntaxHighlighting(start, end, colour));
        }
    }
}
