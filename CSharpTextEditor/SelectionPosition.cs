namespace CSharpTextEditor
{
    internal class SelectionPosition : ISelectionPosition
    {
        private int previousMaxColumnNumber;

        public LinkedListNode<string> Line { get; set; }

        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public SelectionPosition(LinkedListNode<string> line, int columnNumber, int lineNumber)
        {
            Line = line;
            ColumnNumber = columnNumber;
            LineNumber = lineNumber;
            previousMaxColumnNumber = -1;
        }

        public bool AtEndOfLine() => ColumnNumber == Line.Value.Length;

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

        public void ShiftToStartOfLine()
        {
            ColumnNumber = 0;
            previousMaxColumnNumber = -1;
        }

        public void ShiftToEndOfLine()
        {
            ColumnNumber = Line.Value.Length;
            previousMaxColumnNumber = -1;
        }

        public void ShiftOneToTheRight()
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
            previousMaxColumnNumber = -1;
        }

        public void ShiftOneToTheLeft()
        {
            if (ColumnNumber > 0)
            {
                ColumnNumber--;
            }
            else if (Line.Previous != null)
            {
                Line = Line.Previous;
                LineNumber--;
                ShiftToEndOfLine();
            }
            previousMaxColumnNumber = -1;
        }

        public void ShiftUpOneLine()
        {
            if (Line.Previous != null)
            {
                Line = Line.Previous;
                LineNumber--;
                if (previousMaxColumnNumber == -1)
                {
                    ColumnNumber = Math.Min(Line.Value.Length, ColumnNumber);
                }
                else
                {
                    ColumnNumber = Math.Min(Line.Value.Length, previousMaxColumnNumber);
                }
            }
        }

        public void ShiftDownOneLine()
        {
            if (Line.Next != null)
            {
                Line = Line.Next;
                LineNumber++;
                if (previousMaxColumnNumber == -1)
                {
                    ColumnNumber = Math.Min(Line.Value.Length, ColumnNumber);
                }
                else
                {
                    ColumnNumber = Math.Min(Line.Value.Length, previousMaxColumnNumber);
                }
            }
        }
    }
}
