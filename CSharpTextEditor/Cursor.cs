using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace CSharpTextEditor
{
    internal class Cursor : IComparable<Cursor>
    {
        private int _previousMaxColumnNumber;

        public LinkedListNode<SourceCodeLine> Line { get; set; }

        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public Cursor(LinkedListNode<SourceCodeLine> line, int columnNumber, int lineNumber)
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

        public Cursor Clone() => new Cursor(Line, ColumnNumber, LineNumber);

        public bool SamePositionAsOther(Cursor other) => ColumnNumber == other.ColumnNumber && LineNumber == other.LineNumber;

        public void CopyFrom(Cursor other)
        {
            Line = other.Line;
            ColumnNumber = other.ColumnNumber;
            LineNumber = other.LineNumber;
        }

        public int CompareTo(Cursor? other)
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

        public static bool operator <(Cursor left, Cursor right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Cursor left, Cursor right)
        {
            return left.CompareTo(right) > 0;
        }

        public void ResetMaxColumnNumber() => _previousMaxColumnNumber = -1;

        public void ShiftToStartOfLine()
        {
            ColumnNumber = 0;
            ResetMaxColumnNumber();
        }

        public void InsertCharacter(char character)
        {
            Line.Value.InsertCharacter(ColumnNumber, character);
            ColumnNumber++;
            ResetMaxColumnNumber();
        }

        public void InsertText(string text)
        {
            Line.Value.InsertText(ColumnNumber, text);
            ColumnNumber += text.Length;
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


        public bool ShiftUpOneLine()
        {
            return ShiftUpLines(1);
        }

        public bool ShiftUpLines(int lineCount)
        {
            bool moved = false;
            while (Line.Previous != null
                && lineCount > 0)
            {
                Line = Line.Previous;
                LineNumber--;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
                lineCount--;
                moved = true;
            }
            return moved;
        }

        public bool ShiftDownOneLine()
        {
            return ShiftDownLines(1);
        }

        public bool ShiftDownLines(int lineCount)
        {
            bool moved = false;
            while (Line.Next != null
                && lineCount > 0)
            {
                Line = Line.Next;
                LineNumber++;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
                lineCount--;
                moved = true;
            }
            return moved;
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
