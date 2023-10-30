using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal partial class SourceCode
    {
        private class SelectionPosition : ISelectionPosition
        {
            public LinkedListNode<string> Line { get; set; }

            public int ColumnNumber { get; set; }

            public int LineNumber { get; set; }

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

        public IEnumerable<string> Lines => _lines;

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
            SelectionPosition start = (SelectionPosition)_selectionStart;
            SelectionPosition end = _selectionEnd;
            int comparison = start.CompareTo(end);
            if (comparison > 0)
            {
                (start, end) = (end, start);
            }
            int startColumnIndex = start.ColumnNumber;
            LinkedListNode<string>? current = start.Line;
            while (current != null)
            {
                if (current == end.Line)
                {
                    string startString = startColumnIndex == 0 ? string.Empty : current.Value.Substring(0, startColumnIndex);
                    string endString = current.Value.Substring(end.ColumnNumber);
                    current.Value = startString + endString;
                    break;
                }
                else if (startColumnIndex == 0)
                {
                    // remove entire line
                    var next = current.Next;
                    _lines.Remove(current);
                    current = next;
                }
                else
                {
                    current.Value = current.Value.Substring(0, startColumnIndex);
                    current = current.Next;
                }
                startColumnIndex = 0;
            }
            
            _selectionEnd = start.Clone();
            _selectionStart = null;
        }

        public void RemoveCharacterBeforePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            else
            {
                if (_selectionEnd.ColumnNumber > 0)
                {
                    if (_selectionEnd.AtEndOfLine())
                    {
                        _selectionEnd.Line.Value = _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber - 1);
                        _selectionEnd.ColumnNumber--;
                    }
                    else if (_selectionEnd.ColumnNumber == 1)
                    {
                        _selectionEnd.Line.Value = _selectionEnd.Line.Value.Substring(1);
                        _selectionEnd.ColumnNumber--;
                    }
                    else
                    {
                        string before = _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber - 1);
                        string after = _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber);
                        _selectionEnd.Line.Value = before + after;
                        _selectionEnd.ColumnNumber--;
                    }
                }
                else if (_selectionEnd.Line.Previous != null)
                {
                    LinkedListNode<string> oldCurrent = _selectionEnd.Line;
                    _selectionEnd = new SelectionPosition(_selectionEnd.Line.Previous, _selectionEnd.Line.Previous.Value.Length, _selectionEnd.LineNumber - 1);
                    _selectionEnd.Line.Value += oldCurrent.Value;
                    _lines.Remove(oldCurrent);
                }
            }
        }

        public void RemoveCharacterAfterPosition()
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

        public void InsertLineBreakAtPosition()
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

        public void InsertCharacterAtPosition(char character)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
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
            if (!_selectionEnd.AtEndOfLine())
            {
                _selectionEnd.ColumnNumber++;
            }
            else if (_selectionEnd.Line.Next != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Next, 0, _selectionEnd.LineNumber + 1);
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
    }
}
