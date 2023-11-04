using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SourceCode
    {
        private class SelectionPosition : ISelectionPosition
        {
            private int lineNumber;

            public LinkedListNode<string> Line { get; set; }

            public int ColumnNumber { get; set; }

            public int LineNumber { get => lineNumber; set => lineNumber = value; }

            public SelectionPosition(LinkedListNode<string> line, int columnNumber, int lineNumber)
            {
                Line = line;
                ColumnNumber = columnNumber;
                LineNumber = lineNumber;
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
        }

        private LinkedList<string> _lines = new LinkedList<string>(new[] { string.Empty });
        private SelectionPosition? _selectionStart;
        private SelectionPosition _selectionEnd;

        public ISelectionPosition? SelectionStart => _selectionStart;

        public ISelectionPosition SelectionEnd => _selectionEnd;

        public string Text
        {
            get => string.Join(Environment.NewLine, _lines);
            set
            {
                _lines = new LinkedList<string>(value.Split(Environment.NewLine));
                LinkedListNode<string>? last = _lines.Last;
                if (last == null)
                {
                    last = _lines.AddLast(string.Empty);
                }
                _selectionEnd = new SelectionPosition(last, last.Value.Length, _lines.Count - 1);
            }
        }

        public IReadOnlyCollection<string> Lines => _lines;

        public SourceCode()
            :this(string.Empty)
        { 
        }

        public SourceCode(string text)
        {
            Text = text;
        }

        public bool IsRangeSelected()
        {
            return _selectionStart != null
                && !_selectionStart.SamePositionAsOther(_selectionEnd);
        }

        public void RemoveSelectedRange()
        {
            // TODO: This needs updating so that the start and end lines get merged
            (SelectionPosition start, SelectionPosition end) = GetFirstAndLastSelectionPositions();
            while (end > start)
            {
                RemoveCharacterBeforePosition(end);
            }
            _selectionStart = null;
        }

        public void RemoveCharacterBeforeActivePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            else
            {
                RemoveCharacterBeforePosition(_selectionEnd);
            }
        }

        private (SelectionPosition first, SelectionPosition last) GetFirstAndLastSelectionPositions()
        {
            return GetFirstAndLastSelectionPositions(_selectionStart, _selectionEnd);
        }

        private (SelectionPosition first, SelectionPosition last) GetFirstAndLastSelectionPositions(SelectionPosition start, SelectionPosition end)
        {
            // selection start and end may be such that the end is before start. This method returns the earliest then the latest selection position in the text
            if (start > end)
            {
                return (end, start);
            }
            return (start, end);
        }

        private void RemoveCharacterBeforePosition(SelectionPosition position)
        {
            if (position.ColumnNumber > 0)
            {
                if (position.AtEndOfLine())
                {
                    position.Line.Value = position.Line.Value.Substring(0, position.ColumnNumber - 1);
                    position.ColumnNumber--;
                }
                else if (position.ColumnNumber == 1)
                {
                    position.Line.Value = position.Line.Value.Substring(1);
                    position.ColumnNumber--;
                }
                else
                {
                    string before = position.Line.Value.Substring(0, position.ColumnNumber - 1);
                    string after = position.Line.Value.Substring(position.ColumnNumber);
                    position.Line.Value = before + after;
                    position.ColumnNumber--;
                }
            }
            else if (position.Line.Previous != null)
            {
                LinkedListNode<string> oldCurrent = position.Line;

                int columnNumber = position.Line.Previous.Value.Length;
                int lineNumber = position.LineNumber - 1;
                position.Line = position.Line.Previous;
                position.ColumnNumber = columnNumber;
                position.LineNumber = lineNumber;
                position.Line.Value += oldCurrent.Value;
                _lines.Remove(oldCurrent);
            }
        }

        public void RemoveCharacterAfterActivePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            else
            {
                if (!_selectionEnd.AtEndOfLine())
                {
                    string before = _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber);
                    string after = _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber + 1);
                    _selectionEnd.Line.Value = before + after;
                }
                else if (_selectionEnd.Line.Next != null)
                {
                    _selectionEnd.Line.Value += _selectionEnd.Line.Next.Value;
                    _lines.Remove(_selectionEnd.Line.Next);
                }
            }
        }

        public void InsertLineBreakAtActivePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            string newLineContents = string.Empty;
            if (!_selectionEnd.AtEndOfLine())
            {
                newLineContents = _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber);
                _selectionEnd.Line.Value = _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber);
                
            }
            var newLine = _lines.AddAfter(_selectionEnd.Line, newLineContents);
            _selectionEnd = new SelectionPosition(newLine, 0, _selectionEnd.LineNumber + 1);
        }

        public void InsertCharacterAtActivePosition(char character)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            if (character == '\t')
            {
                InsertStringAtActivePosition("   ");
                return;
            }
            if (_selectionEnd.AtEndOfLine())
            {
                _selectionEnd.Line.Value += character;
                _selectionEnd.ColumnNumber++;
            }
            else
            {

                _selectionEnd.Line.Value = string.Concat(
                    _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber),
                    character,
                    _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber));
                _selectionEnd.ColumnNumber++;
            }
        }

        public void InsertStringAtActivePosition(string text)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            text = text.Replace("\t", "   ");
            using (StringReader sr = new StringReader(text))
            {
                string? currentLine = sr.ReadLine();
                while (currentLine != null)
                {
                    if (_selectionEnd.AtEndOfLine())
                    {
                        _selectionEnd.Line.Value += currentLine;
                    }
                    else
                    {
                        _selectionEnd.Line.Value = string.Concat(
                            _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber),
                            currentLine,
                            _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber));
                    }
                    _selectionEnd.ColumnNumber += currentLine.Length;
                    currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        InsertLineBreakAtActivePosition();
                    }
                }
            }
        }

        public void ShiftActivePositionUpOneLine(bool selection)
        {
            UpdateSelectionStart(selection);
            if (_selectionEnd.Line.Previous != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Previous, Math.Min(_selectionEnd.Line.Previous.Value.Length, _selectionEnd.ColumnNumber), _selectionEnd.LineNumber - 1);
            }
        }

        public void ShiftActivePositionDownOneLine(bool selection)
        {
            UpdateSelectionStart(selection);
            if (_selectionEnd.Line.Next != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Next, Math.Min(_selectionEnd.Line.Next.Value.Length, _selectionEnd.ColumnNumber), _selectionEnd.LineNumber + 1);
            }
        }

        public void ShiftActivePositionToTheLeft(bool selection)
        {
            UpdateSelectionStart(selection);
            if (_selectionEnd.ColumnNumber > 0)
            {
                _selectionEnd.ColumnNumber--;
            }
            else if (_selectionEnd.Line.Previous != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Previous, _selectionEnd.Line.Previous.Value.Length, _selectionEnd.LineNumber - 1);
            }
        }

        public void ShiftActivePositionToTheRight(bool selection = false)
        {
            UpdateSelectionStart(selection);
            ShiftPositionToTheRight(_selectionEnd);
        }

        private void ShiftPositionToTheRight(SelectionPosition position)
        {
            if (!position.AtEndOfLine())
            {
                position.ColumnNumber++;
            }
            else if (position.Line.Next != null)
            {
                position.Line = position.Line.Next;
                position.ColumnNumber = 0;
                position.LineNumber++;
            }
        }

        public void ShiftActivePositionToEndOfLine(bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ColumnNumber = _selectionEnd.Line.Value.Length;
        }

        public void ShiftActivePositionToStartOfLine(bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ColumnNumber = 0;
        }

        private void UpdateSelectionStart(bool selection)
        {
            if (!selection)
            {
                _selectionStart = null;
            }
            else if (_selectionStart == null)
            {
                _selectionStart = _selectionEnd.Clone();
            }
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            _selectionStart = null;
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == lineNumber)
                {
                    _selectionEnd = new SelectionPosition(current, Math.Min(columnNumber, current.Value.Length), lineNumber);
                    return;
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                _selectionEnd = new SelectionPosition(_lines.Last, Math.Min(columnNumber, _lines.Last.Value.Length), _lines.Count - 1);
            }
        }

        public void SelectRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine == endLine
                && startColumn == endColumn)
            {
                SetActivePosition(endLine, endColumn);
                return;
            }
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count == startLine)
                {
                    _selectionStart = new SelectionPosition(current, Math.Min(startColumn, current.Value.Length), startLine);
                }
                if (count == endLine)
                {
                    _selectionEnd = new SelectionPosition(current, Math.Min(endColumn, current.Value.Length), endLine);
                }
                count++;
                current = current.Next;
            }
            // TODO: What to do if we don't find it
        }

        public void SelectAll()
        {
            _selectionStart = new SelectionPosition(_lines.First, 0, 0);
            _selectionEnd = new SelectionPosition(_lines.Last, _lines.Last.Value.Length, _lines.Count - 1);
        }

        public string GetSelectedText()
        {
            if (_selectionStart == null)
            {
                return string.Empty;
            }
            (SelectionPosition start, SelectionPosition end) = GetFirstAndLastSelectionPositions(_selectionStart.Clone(), _selectionEnd.Clone());
            // TODO: Do this line by line instead of character by character
            StringBuilder sb = new StringBuilder();
            while (start < end)
            {
                if (start.AtEndOfLine())
                {
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(start.Line.Value[start.ColumnNumber]);
                }
                ShiftPositionToTheRight(start);
            }
            return sb.ToString();
        }
    }
}
