using CSharpTextEditor.Source;
using System.Collections.Generic;

namespace CSharpTextEditor.Languages
{
    public class SyntaxHighlightingCollection
    {
        public IReadOnlyCollection<SyntaxHighlighting> Highlightings { get; }

        public IReadOnlyCollection<SyntaxDiagnostic> Diagnostics { get; }

        public IReadOnlyCollection<(int, int)> BlockLines { get; }

        public SyntaxHighlightingCollection(IReadOnlyCollection<SyntaxHighlighting> highlightings, IReadOnlyCollection<SyntaxDiagnostic> diagnostics, IReadOnlyCollection<(int, int)> blockLines)
        {
            Highlightings = highlightings;
            Diagnostics = diagnostics;
            BlockLines = blockLines;
        }
    }
}