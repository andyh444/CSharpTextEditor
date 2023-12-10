using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public SourceCodePosition GetPosition() => new SourceCodePosition(LineNumber, ColumnNumber);

        public string GetLineValue() => Line.Value.Text;

        public int GetLineLength() => GetLineValue().Length;

        public bool AtStartOfLine() => ColumnNumber == 0;

        public bool AtEndOfLine() => ColumnNumber == GetLineLength();

        public Cursor Clone() => new Cursor(Line, ColumnNumber, LineNumber);

        public bool SamePositionAsOther(Cursor other) => GetPosition().Equals(other.GetPosition());

        public void CopyFrom(Cursor other)
        {
            Line = other.Line;
            ColumnNumber = other.ColumnNumber;
            LineNumber = other.LineNumber;
        }

        public int CompareTo(Cursor other)
        {
            if (other == null)
            {
                throw new NullReferenceException();
            }
            return GetPosition().CompareTo(other.GetPosition());
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

        public bool ShiftToHome()
        {
            int firstNonWhiteSpaceIndex = Line.Value.FirstNonWhiteSpaceIndex;
            if (ColumnNumber == firstNonWhiteSpaceIndex)
            {
                ColumnNumber = 0;
            }
            else
            {
                ColumnNumber = firstNonWhiteSpaceIndex;
            }
            ResetMaxColumnNumber();
            return true;
        }

        public void InsertLineBreak()
        {
            string newLineContents = string.Empty;
            if (!AtEndOfLine())
            {
                newLineContents = Line.Value.GetStringAfterPosition(ColumnNumber);
                Line.Value.Text = Line.Value.GetStringBeforePosition(ColumnNumber);
            }
            LinkedListNode<SourceCodeLine> newLine = Line.List.AddAfter(Line, new SourceCodeLine(newLineContents));
            Line = newLine;
            LineNumber++;
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

        public bool ShiftToEndOfLine()
        {
            ColumnNumber = GetLineLength();
            ResetMaxColumnNumber();
            return true;
        }

        public bool ShiftOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter)
        {
            if (AtEndOfLine())
            {
                return ShiftOneCharacterToTheRight();
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
                        return true;
                    }
                    previousTokenStart = tokenStart;
                }
                return true;
            }
        }

        public bool ShiftOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter)
        {
            if (AtStartOfLine())
            {
                return ShiftOneCharacterToTheLeft();
            }
            else
            {
                int previousTokenEnd = GetLineLength();
                foreach ((int tokenStart, int tokenEnd) in syntaxHighlighter.GetSpansFromTextLine(GetLineValue()).Reverse())
                {
                    if (ColumnNumber <= previousTokenEnd
                        && ColumnNumber > tokenStart)
                    {
                        ColumnNumber = tokenStart;
                        return true;
                    }
                    previousTokenEnd = tokenStart;
                }
                ColumnNumber = 0;
                return true;
            }
        }

        public bool ShiftOneCharacterToTheRight()
        {
            ResetMaxColumnNumber();
            if (!AtEndOfLine())
            {
                ColumnNumber++;
                return true;
            }
            else if (Line.Next != null)
            {
                Line = Line.Next;
                LineNumber++;
                ColumnNumber = 0;
                return true;
            }
            return false;
        }

        public bool ShiftOneCharacterToTheLeft()
        {
            ResetMaxColumnNumber();
            if (!AtStartOfLine())
            {
                ColumnNumber--;
                return true;
            }
            else if (Line.Previous != null)
            {
                Line = Line.Previous;
                LineNumber--;
                ShiftToEndOfLine();
                return true;
            }
            return false;
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
