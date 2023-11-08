using System.Diagnostics.CodeAnalysis;

namespace CSharpTextEditor
{
    public class SyntaxHighlightingEqualityComparer : IEqualityComparer<SyntaxHighlighting>
    {
        public bool Equals(SyntaxHighlighting? x, SyntaxHighlighting? y)
        {
            bool xIsNull = x == null;
            bool yIsNull = y == null;
            if (xIsNull != yIsNull)
            {
                return false;
            }
            if (x == null)
            {
                return true;
            }
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] SyntaxHighlighting obj)
        {
            return obj.GetHashCode();
        }
    }

    public class SyntaxHighlighting : IEquatable<SyntaxHighlighting>
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

        public bool Equals(SyntaxHighlighting? other)
        {
            if (other == null)
            {
                return false;
            }
            return Start.Equals(other.Start)
                && End.Equals(other.End)
                && Colour.Equals(other.Colour);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End, Colour);
        }
    }
}
