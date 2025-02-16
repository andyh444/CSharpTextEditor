﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public interface ISyntaxHighlighter
    {
        SyntaxHighlightingCollection GetHighlightings(IEnumerable<string> sourceLines, SyntaxPalette palette);

        IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition);

        IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition);

        IEnumerable<CodeCompletionSuggestion> GetCodeCompletionSuggestions(string textLine, int position, SyntaxPalette palette);

        CodeCompletionSuggestion GetSuggestionAtPosition(int characterPosition, SyntaxPalette palette);
    }
}
