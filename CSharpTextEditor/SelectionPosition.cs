﻿namespace CSharpTextEditor
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

        public bool AtStartOfLine() => ColumnNumber == 0;

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

        public void ResetMaxColumnNumber() => previousMaxColumnNumber = -1;

        public void ShiftToStartOfLine()
        {
            ColumnNumber = 0;
            ResetMaxColumnNumber();
        }

        public void ShiftToEndOfLine()
        {
            ColumnNumber = Line.Value.Length;
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
                bool reachedEndOfWord = false;
                while(!AtEndOfLine())
                {
                    char currentChar = Line.Value[ColumnNumber];
                    if (reachedEndOfWord != char.IsLetterOrDigit(currentChar))
                    {
                        ShiftOneCharacterToTheRight();
                    }
                    else
                    {
                        if (!reachedEndOfWord)
                        {
                            reachedEndOfWord = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
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

        public void ShiftOneWordToTheLeft()
        {
            if (AtStartOfLine())
            {
                ShiftOneCharacterToTheRight();
            }
            else
            {
                bool reachedEndOfWord = false;
                while (!AtStartOfLine())
                {
                    char currentChar = Line.Value[ColumnNumber - 1];
                    if (reachedEndOfWord != char.IsLetterOrDigit(currentChar))
                    {
                        ShiftOneCharacterToTheLeft();
                    }
                    else
                    {
                        if (!reachedEndOfWord)
                        {
                            reachedEndOfWord = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
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
                if (previousMaxColumnNumber == -1)
                {
                    previousMaxColumnNumber = ColumnNumber;
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
                    previousMaxColumnNumber = ColumnNumber;
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
