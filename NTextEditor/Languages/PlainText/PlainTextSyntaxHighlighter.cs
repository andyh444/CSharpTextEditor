using NTextEditor.View;
using System;
using System.Collections.Generic;

namespace NTextEditor.Languages.PlainText
{
    internal class PlainTextSyntaxHighlighter : ISyntaxHighlighter
    {
        public SyntaxHighlightingCollection GetHighlightings(SyntaxPalette palette)
            => new SyntaxHighlightingCollection([], [], []);

        public IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette, out int argumentIndex)
        {
            argumentIndex = -1;
            return [];
        }

        public CodeCompletionSuggestion? GetSymbolInfoAtPosition(int characterPosition, SyntaxPalette palette)
            => null;

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
            => [];

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
            => [];

        public void Update(IEnumerable<string> sourceLines)
        {
        }
    }
}
