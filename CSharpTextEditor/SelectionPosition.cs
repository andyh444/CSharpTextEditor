using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace CSharpTextEditor
{
    internal class SelectionPosition : ISelectionPosition
    {
        private int _previousMaxColumnNumber;

        public LinkedListNode<string> Line { get; set; }

        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public SelectionPosition(LinkedListNode<string> line, int columnNumber, int lineNumber)
        {
            Line = line;
            ColumnNumber = columnNumber;
            LineNumber = lineNumber;
            _previousMaxColumnNumber = -1;
        }

        public string GetLineValue() => Line.Value;

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

        public void ShiftOneWordToTheRight()
        {
            if (AtEndOfLine())
            {
                ShiftOneCharacterToTheRight();
            }
            else
            {
                int previousTokenStart = 0;
                // TODO: This will only work for C# text, needs to be generalised
                foreach (var token in CSharpSyntaxTree.ParseText(Line.Value).GetRoot().DescendantTokens())
                {
                    if (ColumnNumber >= previousTokenStart
                        && ColumnNumber < token.Span.Start)
                    {
                        ColumnNumber = token.Span.Start;
                        return;
                    }
                    previousTokenStart = token.Span.Start;
                }
            }
        }

        public void ShiftOneWordToTheLeft()
        {
            if (AtStartOfLine())
            {
                ShiftOneCharacterToTheLeft();
            }
            else
            {
                int previousTokenEnd = GetLineLength();
                // TODO: This will only work for C# text, needs to be generalised
                foreach (var token in CSharpSyntaxTree.ParseText(Line.Value).GetRoot().DescendantTokens().Reverse())
                {
                    if (ColumnNumber <= previousTokenEnd
                        && ColumnNumber > token.Span.Start)
                    {
                        ColumnNumber = token.Span.Start;
                        return;
                    }
                    previousTokenEnd = token.Span.Start;
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
            if (Line.Previous != null)
            {
                Line = Line.Previous;
                LineNumber--;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
            }
        }

        public void ShiftDownOneLine()
        {
            if (Line.Next != null)
            {
                Line = Line.Next;
                LineNumber++;
                ColumnNumber = Math.Min(GetLineLength(), GetCurrentOrPreviousMaxColumnNumber());
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
