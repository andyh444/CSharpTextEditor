namespace CSharpTextEditor
{
    public class SyntaxHighlighting
    {
        public SourceCodePosition Start { get; }

        public SourceCodePosition End { get; }

        public Color Colour { get; }

        public SyntaxHighlighting(SourceCodePosition start, SourceCodePosition end, Color colour)
        {
            Start = start;
            End = end;
            Colour = colour;
        }

        public bool IsOnLine(int lineNumber)
        {
            return lineNumber >= Start.LineNumber && lineNumber <= End.LineNumber;
        }
    }
}
