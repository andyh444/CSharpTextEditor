using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NTextEditor.Languages.Common;
using NTextEditor.Source;
using NTextEditor.Utility;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NTextEditor.Languages.Common.CodeAnalysisHelper;
using SymbolVisitor = NTextEditor.Languages.Common.SymbolVisitor;

namespace NTextEditor.Languages.VisualBasic
{
    internal class VisualBasicSyntaxHighlighter : ISyntaxHighlighter, ICodeExecutor
    {
        private CompilationContainer? _compilation;
        private readonly bool isLibrary;

        internal VisualBasicSyntaxHighlighter(bool isLibrary = true)
        {
            // little hack to ensure Microsoft.VisualBasic is loaded
            var t = typeof(Microsoft.VisualBasic.Strings);

            this.isLibrary = isLibrary;
        }

        public SyntaxHighlightingCollection GetHighlightings(SyntaxPalette palette)
        {
            if (_compilation == null)
            {
                throw new CSharpTextEditorException("Must call Update before calling GetHighlightings");
            }
            return CodeAnalysisHelper.GetHighlightings(_compilation, palette);
        }

        public IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette, out int argumentIndex)
        {
            // TODO
            argumentIndex = -1;
            return [];
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

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansAfterPosition(characterPosition, _compilation);

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansBeforePosition(characterPosition, _compilation);

        public void Update(IEnumerable<string> sourceLines)
        {
            (string sourceText, IReadOnlyList<int> cumulativeLineLengths) = sourceLines.ToText();
            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(sourceText);

            if (_compilation == null)
            {
                _compilation = CompilationContainer.FromTree(tree, GetReferences(), cumulativeLineLengths, isLibrary);
            }
            else
            {
                _compilation = _compilation.WithNewTree(tree, cumulativeLineLengths);
            }
        }

        public void Execute(TextWriter output)
        {
            CodeAnalysisHelper.Execute(_compilation, output);
        }

        private ISymbol? GetSymbol(SyntaxToken sourceToken, CompilationContainer compilation, out string? name, out bool isConstructor, out int argumentIndex)
        {
            name = null;
            isConstructor = false;
            argumentIndex = -1;

            SyntaxNode? currentNode = sourceToken.Parent;

            while (currentNode != null)
            {
                var currentSymbolInfo = compilation.SemanticModel.GetSymbolInfo(currentNode);
                if (currentSymbolInfo.Symbol != null)
                {
                    return currentSymbolInfo.Symbol;
                }
                var declaredSymbol = compilation.SemanticModel.GetDeclaredSymbol(currentNode);
                if (declaredSymbol != null)
                {
                    return declaredSymbol;
                }
                switch (currentNode)
                {
                    case MemberAccessExpressionSyntax maes:
                        currentNode = maes.Expression is MemberAccessExpressionSyntax maes2
                            ? maes2.Name
                            : (SyntaxNode)maes.Expression;
                        break;

                    // TODO: Other cases
                    case IdentifierNameSyntax ins:
                        if (ins.Parent != null)
                        {
                            currentNode = ins.Parent;
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    default:
                        return SymbolVisitor.FindSymbolsWithName(currentNode.ToString(), compilation.SemanticModel).FirstOrDefault();
                }
            }

            return null;
        }

        private CodeCompletionSuggestion? SymbolToSuggestion(ISymbol symbol, SyntaxPalette syntaxPalette, bool isDeclaration = false)
        {
            string name = symbol.Name;
            SymbolType type = symbol switch
            {
                IMethodSymbol => SymbolType.Method,
                IPropertySymbol => SymbolType.Property,
                INamedTypeSymbol => SymbolType.Class, // TODO: Add other cases
                INamespaceSymbol => SymbolType.Namespace,
                IFieldSymbol => SymbolType.Field,
                ILocalSymbol or IParameterSymbol => SymbolType.Local,
                _ => SymbolType.None
            };
            HighlightedToolTipBuilder builder = new HighlightedToolTipBuilder(syntaxPalette);

            var displayFormat = new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeType,
                SymbolDisplayDelegateStyle.NameOnly,
                SymbolDisplayExtensionMethodStyle.Default,
                SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
                SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
                SymbolDisplayLocalOptions.IncludeType,
                SymbolDisplayKindOptions.IncludeMemberKeyword,
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            );

            foreach (var part in symbol.ToDisplayParts(displayFormat))
            {
                Color colour = part.Kind switch
                {
                    SymbolDisplayPartKind.Keyword => syntaxPalette.BlueKeywordColour,
                    SymbolDisplayPartKind.ClassName
                        or SymbolDisplayPartKind.InterfaceName
                        or SymbolDisplayPartKind.StructName => syntaxPalette.TypeColour,
                    SymbolDisplayPartKind.MethodName => syntaxPalette.MethodColour,
                    _ => syntaxPalette.DefaultTextColour
                };
                builder.Add(part.ToString(), colour);
            }

            return new CodeCompletionSuggestion(name, type, builder, isDeclaration);
        }
    }
}
