namespace CSharpTextEditor
{
    public class SyntaxHighlightingCollection
    {
        public IReadOnlyCollection<SyntaxHighlighting> Highlightings { get; }

        public IReadOnlyCollection<(int line, int startColumn, int endColumn, string message)> Errors { get; }

        public SyntaxHighlightingCollection(IReadOnlyCollection<SyntaxHighlighting> highlightings, IReadOnlyCollection<(int line, int startColumn, int endColumn, string message)> errors)
        {
            Highlightings = highlightings;
            Errors = errors;
        }
    }
}
