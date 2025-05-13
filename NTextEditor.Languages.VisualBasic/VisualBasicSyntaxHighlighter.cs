using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using NTextEditor.Languages.Common;
using NTextEditor.Source;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NTextEditor.Languages.Common.CodeAnalysisHelper;

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
            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            IReadOnlyList<int> cumulativeLineLengths = _compilation.CumulativeLineLengths;

            VisualBasicSyntaxHighlightingWalker highlighter = new VisualBasicSyntaxHighlightingWalker(_compilation.SemanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting, cumulativeLineLengths),
                (span) => blockLines.Add((SourceCodePosition.FromCharacterIndex(span.Start, cumulativeLineLengths).LineNumber, SourceCodePosition.FromCharacterIndex(span.End, cumulativeLineLengths).LineNumber)),
                palette);
            highlighter.Visit(_compilation.CurrentTree.GetRoot());

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), _compilation.GetDiagnostics().Result, blockLines);
        }

        public IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette, out int argumentIndex)
        {
            // TODO
            argumentIndex = -1;
            return [];
        }

        public CodeCompletionSuggestion? GetSymbolInfoAtPosition(int characterPosition, SyntaxPalette palette)
        {
            // TODO
            return null;
        }

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansAfterPosition(characterPosition, _compilation);

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
            => CodeAnalysisHelper.GetSymbolSpansBeforePosition(characterPosition, _compilation);

        public void Update(IEnumerable<string> sourceLines)
        {
            (string sourceText, IImmutableList<int> cumulativeLineLengths) = GetText(sourceLines);
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
    }
}
