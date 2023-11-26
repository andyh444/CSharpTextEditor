using System.Collections.Generic;

namespace CSharpTextEditor
{
    public class SyntaxHighlightingCollection
    {
        public IReadOnlyCollection<SyntaxHighlighting> Highlightings { get; }

        public IReadOnlyCollection<(SourceCodePosition start, SourceCodePosition end, string message)> Errors { get; }

        public IReadOnlyCollection<(int, int)> BlockLines { get; }

        public SyntaxHighlightingCollection(IReadOnlyCollection<SyntaxHighlighting> highlightings, IReadOnlyCollection<(SourceCodePosition start, SourceCodePosition end, string message)> errors, IReadOnlyCollection<(int, int)> blockLines)
        {
            Highlightings = highlightings;
            Errors = errors;
            BlockLines = blockLines;
        }
    }
}