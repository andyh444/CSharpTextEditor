﻿using System.Text;

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

        public string GetLineValue() => GetLineValueFromNode(Line);

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
                bool reachedEndOfWord = false;
                while(!AtEndOfLine())
                {
                    char currentChar = GetLineValue()[ColumnNumber];
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
                ShiftOneCharacterToTheLeft();
            }
            else
            {
                bool reachedEndOfWord = false;
                while (!AtStartOfLine())
                {
                    char currentChar = GetLineValue()[ColumnNumber - 1];
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

        public static string GetLineValueFromNode(LinkedListNode<string> node)
        {
            if (string.IsNullOrEmpty(node.Value))
            {
                if (node.Previous != null)
                {
                    StringBuilder sb = new StringBuilder();
                    StringReader sr = new StringReader(GetLineValueFromNode(node.Previous));

                    Span<char> buffer = new Span<char>(new char[1]);
                    int currentReadAmount = sr.Read(buffer);
                    while (currentReadAmount > 0)
                    {
                        char currentChar = buffer[0];
                        if (char.IsWhiteSpace(currentChar))
                        {
                            sb.Append(currentChar);
                        }
                        else if (currentChar == '{')
                        {
                            sb.Append("   ");
                        }
                        else
                        {
                            break;
                        }
                        currentReadAmount = sr.Read(buffer);
                    }
                    return sb.ToString();
                }
            }
            return node.Value;
        }
    }
}
