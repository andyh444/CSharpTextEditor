using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NTextEditor.Source
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
            Queue<string> lineQueue = new Queue<string>(lines);
            int index = 0;
            for (int i = 0; i < LineNumber; i++)
            {
                if (lineQueue.Count == 0)
                {
                    return -1;
                }
                index += lineQueue.Dequeue().Length + Environment.NewLine.Length;
            }
            if (lineQueue.Count == 0)
            {
                return -1;
            }
            string currentLine = lineQueue.Dequeue();
            if (ColumnNumber > currentLine.Length)
            {
                return -1;
            }
            return index + ColumnNumber;
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

        public static SourceCodePosition FromCharacterIndex(int characterIndex, IReadOnlyList<int> cumulativeLineLengths)
        {
            if (cumulativeLineLengths.Count == 0)
            {
                throw new Exception("Cannot get character index with no lines");
            }
            if (cumulativeLineLengths.Count == 1)
            {
                if (characterIndex < cumulativeLineLengths[0])
                {
                    return new SourceCodePosition(0, characterIndex);
                }
                else
                {
                    throw new Exception($"Couldn't find position for character index : {characterIndex}");
                }
            }

            int lo = 0;
            int hi = cumulativeLineLengths.Count;

            while (lo <= hi)
            {
                // i might overflow if lo and hi are both large positive numbers.
                int i = (lo + hi) / 2;

                int current = cumulativeLineLengths[i];
                int previous = i > 0 ? cumulativeLineLengths[i - 1] : 0;
                if (characterIndex == current)
                {
                    return new SourceCodePosition(i + 1, 0);
                }
                if (characterIndex < current
                    && characterIndex >= previous)
                {
                    return new SourceCodePosition(i, characterIndex - previous);
                }
                if (characterIndex > current)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
            throw new Exception($"Couldn't find position for character index : {characterIndex}");
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

        public static bool operator ==(SourceCodePosition left, SourceCodePosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SourceCodePosition left, SourceCodePosition right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(SourceCodePosition left, SourceCodePosition right)
        {
            return left.CompareTo(right) >= 0;
        }

        public override string ToString()
        {
            return $"(C:{ColumnNumber}, L:{LineNumber})";
        }

        public static bool TryParse(string text, out SourceCodePosition? position)
        {
            position = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var match = Regex.Match(text, @"^\(C:(\d+), L:(\d+)\)$");

            if (match.Success
                && int.TryParse(match.Groups[1].Value, out int columnNumber)
                && int.TryParse(match.Groups[2].Value, out int lineNumber))
            {
                position = new SourceCodePosition(lineNumber, columnNumber);
                return true;
            }

            return false;
        }
    }
}
