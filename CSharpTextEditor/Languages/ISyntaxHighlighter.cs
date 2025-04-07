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

        /// <summary>
        /// Returns a list of code completion suggestions at the given position for use by the code completion form and method tooltip
        /// </summary>
        /// <param name="characterPosition">the character position (where newlines are just treated as characters)</param>
        /// <param name="palette">the palette to use to build the suggestions</param>
        /// <param name="argumentIndex">if the suggestions represent a method, the argument index will be the argument index at the current position</param>
        /// <returns>a list of relevant code completion suggestions</returns>
        IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette, out int argumentIndex);

        /// <summary>
        /// Returns the symbol info at the given position for use by the hover tooltip
        /// </summary>
        CodeCompletionSuggestion? GetSymbolInfoAtPosition(int characterPosition, SyntaxPalette palette);
    }
}
