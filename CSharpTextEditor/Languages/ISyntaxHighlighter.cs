using CSharpTextEditor.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Languages
{
    public interface ISyntaxHighlighter
    {
        void Update(IEnumerable<string> sourceLines);

        SyntaxHighlightingCollection GetHighlightings(SyntaxPalette palette);

        IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition);

        IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition);

        IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette);
    }
}
