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
        private SelectionPosition _currentPosition;

        public ISelectionPosition CurrentPosition => _currentPosition;

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
                _currentPosition = new SelectionPosition(last, last.Value.Length, _lines.Count - 1);
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
            if (_currentPosition.ColumnNumber > 0)
            {
                if (_currentPosition.AtEndOfLine())
                {
                    _currentPosition.Line.Value = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber - 1);
                    _currentPosition.ColumnNumber--;
                }
                else if (_currentPosition.ColumnNumber == 1)
                {
                    _currentPosition.Line.Value = _currentPosition.Line.Value.Substring(1);
                    _currentPosition.ColumnNumber--;
                }
                else
                {
                    string before = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber - 1);
                    string after = _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber);
                    _currentPosition.Line.Value = before + after;
                    _currentPosition.ColumnNumber--;
                }
            }
            else if (_currentPosition.Line.Previous != null)
            {
                LinkedListNode<string> oldCurrent = _currentPosition.Line;
                _currentPosition = new SelectionPosition(_currentPosition.Line.Previous, _currentPosition.Line.Previous.Value.Length, _currentPosition.LineNumber - 1);
                _currentPosition.Line.Value += oldCurrent.Value;
                _lines.Remove(oldCurrent);
            }
        }

        public void RemoveCharacterAfterPosition()
        {
            if (!_currentPosition.AtEndOfLine())
            {
                string before = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber);
                string after = _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber + 1);
                _currentPosition.Line.Value = before + after;
            }
        }

        public void InsertLineBreakAtPosition()
        {
            string newLineContents = string.Empty;
            if (!_currentPosition.AtEndOfLine())
            {
                newLineContents = _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber);
                _currentPosition.Line.Value = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber);
                
            }
            var newLine = _lines.AddAfter(_currentPosition.Line, newLineContents);
            _currentPosition = new SelectionPosition(newLine, 0, _currentPosition.LineNumber + 1);
        }

        public void InsertCharacterAtPosition(char character)
        {
            if (_currentPosition.AtEndOfLine())
            {
                _currentPosition.Line.Value += character;
                _currentPosition.ColumnNumber++;
            }
            else
            {

                _currentPosition.Line.Value = string.Concat(
                    _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber),
                    character,
                    _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber));
                _currentPosition.ColumnNumber++;
            }
        }

        public void ShiftActivePositionUpOneLine()
        {
            if (_currentPosition.Line.Previous != null)
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line.Previous, Math.Min(_currentPosition.Line.Previous.Value.Length, _currentPosition.ColumnNumber), _currentPosition.LineNumber - 1);
            }
        }

        public void ShiftActivePositionDownOneLine()
        {
            if (_currentPosition.Line.Next != null)
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line.Next, Math.Min(_currentPosition.Line.Next.Value.Length, _currentPosition.ColumnNumber), _currentPosition.LineNumber + 1);
            }
        }

        public void ShiftActivePositionToTheLeft()
        {
            if (_currentPosition.ColumnNumber > 0)
            {
                _currentPosition.ColumnNumber--;
            }
            else if (_currentPosition.Line.Previous != null)
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line.Previous, _currentPosition.Line.Previous.Value.Length, _currentPosition.LineNumber - 1);
            }
        }

        public void ShiftActivePositionToTheRight()
        {
            if (!_currentPosition.AtEndOfLine())
            {
                _currentPosition.ColumnNumber++;
            }
            else if (_currentPosition.Line.Next != null)
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line.Next, 0, _currentPosition.LineNumber + 1);
            }
        }

        public void ShiftActivePositionToEndOfLine()
        {
            _currentPosition.ColumnNumber = _currentPosition.Line.Value.Length;
        }

        public void ShiftActivePositionToStartOfLine()
        {
            _currentPosition.ColumnNumber = 0;
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == lineNumber)
                {
                    _currentPosition = new SelectionPosition(current, Math.Min(columnNumber, current.Value.Length), lineNumber);
                    return;
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                _currentPosition = new SelectionPosition(_lines.Last, Math.Min(columnNumber, _lines.Last.Value.Length), _lines.Count - 1);
            }
        }
    }
}
