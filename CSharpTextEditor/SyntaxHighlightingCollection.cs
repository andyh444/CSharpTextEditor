namespace CSharpTextEditor
{
    public class SyntaxHighlightingCollection
    {
        public IReadOnlyCollection<SyntaxHighlighting> Highlightings { get; }

        public IReadOnlyCollection<(SourceCodePosition start, SourceCodePosition end, string message)> Errors { get; }

        public SyntaxHighlightingCollection(IReadOnlyCollection<SyntaxHighlighting> highlightings, IReadOnlyCollection<(SourceCodePosition start, SourceCodePosition end, string message)> errors)
        {
            Highlightings = highlightings;
            Errors = errors;
        }
    }
}
