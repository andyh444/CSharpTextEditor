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
using static NTextEditor.Languages.Common.CodeAnalysisHelper;
using NTextEditor.Languages.Common;

namespace NTextEditor.Languages.CSharp
{
    internal class CSharpSyntaxHighlighter : ISyntaxHighlighter, ICodeExecutor
    {
        private CompilationContainer? _compilation;
        private readonly bool isLibrary;

        internal CSharpSyntaxHighlighter(bool isLibrary = true)
        {
            this.isLibrary = isLibrary;
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
            var task = _compilation.GetDiagnostics();
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(_compilation.SemanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
                palette);
            highlighter.Visit(_compilation.CurrentTree.GetRoot());

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), task.Result, blockLines);
        }

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansBeforePosition(characterPosition, _compilation);

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansAfterPosition(characterPosition, _compilation);

        public void Execute(TextWriter output)
        {
            CodeAnalysisHelper.Execute(_compilation, output);
        }
    }
}
