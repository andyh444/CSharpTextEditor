using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace CSharpTextEditor
{
    internal class SelectionPosition : ISelectionPosition
    {
        private int _previousMaxColumnNumber;

        public LinkedListNode<SourceCodeLine> Line { get; set; }

        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public SelectionPosition(LinkedListNode<SourceCodeLine> line, int columnNumber, int lineNumber)
        {
            Line = line;
            ColumnNumber = columnNumber;
            LineNumber = lineNumber;
            _previousMaxColumnNumber = -1;
        }

        public string GetLineValue() => Line.Value.Text;

        public int GetLineLength() => GetLineValue().Length;

        public bool AtStartOfLine() => ColumnNumber == 0;

        public bool AtEndOfLine() => ColumnNumber == GetLineLength();

        public SelectionPosition Clone() => new SelectionPosition(Line, ColumnNumber, LineNumber);

        public bool SamePositionAsOther(SelectionPosition other) => ColumnNumber == other.ColumnNumber && LineNumber == other.LineNumber;

        public int CompareTo(ISelectionPosition? other)
        {
            if (other == null)
            {
                throw new NullReferenceException();
            }
            if (LineNumber == other.LineNumber)
            {
                return ColumnNumber.CompareTo(other.ColumnNumber);
            }
            return LineNumber.CompareTo(other.LineNumber);
        }

        public static bool operator <(SelectionPosition left, SelectionPosition right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(SelectionPosition left, SelectionPosition right)
        {
            return left.CompareTo(right) > 0;
        }

        public void ResetMaxColumnNumber() => _previousMaxColumnNumber = -1;

        public void ShiftToStartOfLine()
        {
            ColumnNumber = 0;
            ResetMaxColumnNumber();
        }

        public void ShiftToEndOfLine()
        {
            ColumnNumber = GetLineLength();
            ResetMaxColumnNumber();
        }

        public void ShiftOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter)
        {
            if (AtEndOfLine())
            {
                ShiftOneCharacterToTheRight();
            }
            else
            {
                int previousTokenStart = 0;
                foreach ((int tokenStart, int tokenEnd) in syntaxHighlighter.GetSpansFromTextLine(GetLineValue()))
                {
                    if (ColumnNumber >= previousTokenStart
                        && ColumnNumber < tokenStart)
                    {
                        ColumnNumber = tokenStart;
                        return;
                    }
                    previousTokenStart = tokenStart;
                }
            }
        }

        public void ShiftOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter)
        {
            if (AtStartOfLine())
            {
                ShiftOneCharacterToTheLeft();
            }
            else
            {
                int previousTokenEnd = GetLineLength();
                // TODO: This will only work for C# text, needs to be generalised
                foreach ((int tokenStart, int tokenEnd) in syntaxHighlighter.GetSpansFromTextLine(GetLineValue()).Reverse())
                {
                    if (ColumnNumber <= previousTokenEnd
                        && ColumnNumber > tokenStart)
                    {
                        ColumnNumber = tokenStart;
                        return;
                    }
                    previousTokenEnd = tokenStart;
                }
                ColumnNumber = 0;
            }
        }

        public void ShiftOneCharacterToTheRight()
        {
            if (!AtEndOfLine())
            {
                ColumnNumber++;
            }
            else if (Line.Next != null)
            {
                Line = Line.Next;
                LineNumber++;
                ShiftToStartOfLine();
            }
            ResetMaxColumnNumber();
        }

        public void ShiftOneCharacterToTheLeft()
        {
            if (!AtStartOfLine())
            {
                ColumnNumber--;
            }
            else if (Line.Previous != null)
            {
                Line = Line.Previous;
                LineNumber--;
                ShiftToEndOfLine();
            }
            ResetMaxColumnNumber();
        }


        public void ShiftUpOneLine()
        {
            ShiftUpLines(1);
        }

        public void ShiftUpLines(int lineCount)
        {
            while (Line.Previous != null
                && lineCount > 0)
            {
                Line = Line.Previous;
                LineNumber--;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
                lineCount--;
            }
        }

        public void ShiftDownOneLine()
        {
            ShiftDownLines(1);
        }

        public void ShiftDownLines(int lineCount)
        {
            while (Line.Next != null
                && lineCount > 0)
            {
                Line = Line.Next;
                LineNumber++;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
                lineCount--;
            }
        }

        private int GetCurrentOrPreviousMaxColumnNumber()
        {
            if (_previousMaxColumnNumber != -1)
            {
                return _previousMaxColumnNumber;
            }
            _previousMaxColumnNumber = ColumnNumber;
            return ColumnNumber;
        }
    }
}
