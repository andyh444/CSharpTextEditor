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

        public void RemoveCharacterBeforePosition()
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

        public void RemoveCharacterAfterPosition()
        {
            if (!_selectionEnd.AtEndOfLine())
            {
                string before = _selectionEnd.Line.Value.Substring(0, _selectionEnd.ColumnNumber);
                string after = _selectionEnd.Line.Value.Substring(_selectionEnd.ColumnNumber + 1);
                _selectionEnd.Line.Value = before + after;
            }
        }

        public void InsertLineBreakAtPosition()
        {
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

        public void ShiftActivePositionUpOneLine()
        {
            if (_selectionEnd.Line.Previous != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Previous, Math.Min(_selectionEnd.Line.Previous.Value.Length, _selectionEnd.ColumnNumber), _selectionEnd.LineNumber - 1);
            }
        }

        public void ShiftActivePositionDownOneLine()
        {
            if (_selectionEnd.Line.Next != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Next, Math.Min(_selectionEnd.Line.Next.Value.Length, _selectionEnd.ColumnNumber), _selectionEnd.LineNumber + 1);
            }
        }

        public void ShiftActivePositionToTheLeft()
        {
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
            if (!selection)
            {
                _selectionStart = null;
            }
            else if (_selectionStart == null)
            {
                _selectionStart = new SelectionPosition(_selectionEnd.Line, _selectionEnd.ColumnNumber, _selectionEnd.LineNumber);
            }
            if (!_selectionEnd.AtEndOfLine())
            {
                _selectionEnd.ColumnNumber++;
            }
            else if (_selectionEnd.Line.Next != null)
            {
                _selectionEnd = new SelectionPosition(_selectionEnd.Line.Next, 0, _selectionEnd.LineNumber + 1);
            }
        }

        public void ShiftActivePositionToEndOfLine()
        {
            _selectionEnd.ColumnNumber = _selectionEnd.Line.Value.Length;
        }

        public void ShiftActivePositionToStartOfLine()
        {
            _selectionEnd.ColumnNumber = 0;
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
