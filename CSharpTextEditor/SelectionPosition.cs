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
                char currentChar = GetLineValue()[ColumnNumber];
                bool isInsideWord = char.IsLetterOrDigit(currentChar);
                bool isInsideSymbol = IsPunctuationOrSymbol(currentChar);
                bool isInWhiteSpace = !isInsideSymbol && !isInsideWord;
                if (isInWhiteSpace)
                {
                    while (!AtEndOfLine())
                    {
                        currentChar = GetLineValue()[ColumnNumber];
                        if (char.IsLetterOrDigit(currentChar))
                        {
                            isInsideWord = true;
                            break;
                        }
                        else if (IsPunctuationOrSymbol(currentChar))
                        {
                            isInsideSymbol = true;
                            break;
                        }
                        else
                        {
                            ShiftOneCharacterToTheRight();
                        }
                    }
                }
                // we are now inside a word
                while (!AtEndOfLine())
                {
                    currentChar = GetLineValue()[ColumnNumber];
                    if ((isInsideWord && char.IsLetterOrDigit(currentChar))
                        || (isInsideSymbol && IsPunctuationOrSymbol(currentChar)))
                    {
                        ShiftOneCharacterToTheRight();
                    }
                    else
                    {
                        return;
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

        private bool IsPunctuationOrSymbol(char c)
        {
            return char.IsPunctuation(c) || char.IsSymbol(c);
        }

        public void ShiftOneWordToTheLeft()
        {
            if (AtStartOfLine())
            {
                ShiftOneCharacterToTheLeft();
            }
            else
            {
                char currentChar = GetLineValue()[ColumnNumber - 1];
                bool isInsideWord = char.IsLetterOrDigit(currentChar);
                bool isInsideSymbol = IsPunctuationOrSymbol(currentChar);
                bool isInWhiteSpace = !isInsideSymbol && !isInsideWord;
                if (isInWhiteSpace)
                {
                    while (!AtStartOfLine())
                    {
                        currentChar = GetLineValue()[ColumnNumber - 1];
                        if (char.IsLetterOrDigit(currentChar))
                        {
                            isInsideWord = true;
                            break;
                        }
                        else if (IsPunctuationOrSymbol(currentChar))
                        {
                            isInsideSymbol = true;
                            break;
                        }
                        else
                        {
                            ShiftOneCharacterToTheLeft();
                        }
                    }
                }
                // we are now inside a word
                while (!AtStartOfLine())
                {
                    currentChar = GetLineValue()[ColumnNumber - 1];
                    if ((isInsideWord && char.IsLetterOrDigit(currentChar))
                        || (isInsideSymbol && IsPunctuationOrSymbol(currentChar)))
                    {
                        ShiftOneCharacterToTheLeft();
                    }
                    else
                    {
                        return;
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
