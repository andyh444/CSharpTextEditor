using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace CSharpTextEditor
{
    internal class SyntaxHighlightingEqualityComparer : IEqualityComparer<SyntaxHighlighting>
    {
        public bool Equals(SyntaxHighlighting x, SyntaxHighlighting y)
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

        public int GetHashCode(SyntaxHighlighting obj)
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

        public bool Equals(SyntaxHighlighting other)
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
            return Start.GetHashCode() ^ End.GetHashCode() ^ Colour.GetHashCode();
        }
    }
}
