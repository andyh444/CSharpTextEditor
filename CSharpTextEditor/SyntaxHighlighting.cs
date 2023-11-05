namespace CSharpTextEditor
{
    public class SyntaxHighlighting
    {
        public int Line { get; }

        public int StartColumn { get; }

        public int EndColumn { get; }

        public Color Colour { get; }

        public SyntaxHighlighting(int line, int startColumn, int endColumn, Color colour)
        {
            Line = line;
            StartColumn = startColumn;
            EndColumn = endColumn;
            Colour = colour;
        }
    }
}
