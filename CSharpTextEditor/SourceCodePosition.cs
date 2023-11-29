using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public struct SourceCodePosition : IEquatable<SourceCodePosition>, IComparable<SourceCodePosition>
    {
        public int LineNumber { get; }

        public int ColumnNumber { get; }

        public SourceCodePosition(int lineNumber, int columnNumber)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public int ToCharacterIndex(IEnumerable<string> lines)
        {
            return lines.Take(LineNumber).Sum(x => x.Length + Environment.NewLine.Length) + ColumnNumber;
        }

        public static SourceCodePosition FromCharacterIndex(int characterIndex, IEnumerable<string> lines)
        {
            int lineIndex = 0;
            int newLineLength = Environment.NewLine.Length;
            foreach (string line in lines)
            {
                if (characterIndex <= line.Length)
                {
                    return new SourceCodePosition(lineIndex, characterIndex);
                }
                characterIndex -= line.Length;
                characterIndex -= newLineLength;
                lineIndex++;
            }
            throw new Exception("Couldn't find position");
        }

        public bool Equals(SourceCodePosition other)
        {
            return LineNumber == other.LineNumber && ColumnNumber == other.ColumnNumber;
        }

        public int CompareTo(SourceCodePosition other)
        {
            if (LineNumber == other.LineNumber)
            {
                return ColumnNumber.CompareTo(other.ColumnNumber);
            }
            return LineNumber.CompareTo(other.LineNumber);
        }

        public static bool operator <(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) > 0;
        }
    }
}
