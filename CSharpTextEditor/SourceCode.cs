using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SourceCode
    {
        private struct SelectionPosition
        {
            public LinkedListNode<string> Line { get; }

            public int ColumnNumber { get; }

            public SelectionPosition(LinkedListNode<string> line, int columnNumber)
            {
                Line = line;
                ColumnNumber = columnNumber;
            }

            public bool AtEndOfLine() => ColumnNumber == Line.Value.Length;
        }

        private LinkedList<string> _lines = new LinkedList<string>(new[] { string.Empty });
        private SelectionPosition _currentPosition = new SelectionPosition();

        public int CurrentColumnNumber => _currentPosition.ColumnNumber;

        public string Text
        {
            get => string.Join(Environment.NewLine, _lines);
            set
            {
                _lines = new LinkedList<string>(value.Split(Environment.NewLine));
                if (_lines.Count == 0)
                {
                    _lines.AddLast(string.Empty);
                }
                _currentPosition = new SelectionPosition(_lines.Last, _lines.Last.Value.Length);
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

        public void RemoveCharacterAtPosition()
        {
            if (_currentPosition.ColumnNumber > 0)
            {
                if (_currentPosition.AtEndOfLine())
                {
                    _currentPosition.Line.Value = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber - 1);
                    _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber - 1);
                }
                else if (_currentPosition.ColumnNumber == 1)
                {
                    _currentPosition.Line.Value = _currentPosition.Line.Value.Substring(1);
                    _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber - 1);
                }
                else
                {
                    string before = _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber - 1);
                    string after = _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber, _currentPosition.Line.Value.Length - _currentPosition.ColumnNumber);
                    _currentPosition.Line.Value = before + after;
                    _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber - 1);
                }
            }
        }

        public void InsertCharacterAtPosition(char character)
        {
            if (_currentPosition.AtEndOfLine())
            {
                _currentPosition.Line.Value += character;
                _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber + 1);
            }
            else
            {

                _currentPosition.Line.Value = string.Concat(
                    _currentPosition.Line.Value.Substring(0, _currentPosition.ColumnNumber),
                    character,
                    _currentPosition.Line.Value.Substring(_currentPosition.ColumnNumber, _currentPosition.Line.Value.Length - _currentPosition.ColumnNumber));
                _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber + 1);
            }
        }

        public void ShiftActivePositionToTheLeft()
        {
            if (_currentPosition.ColumnNumber > 0)
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber - 1);
            }
        }

        public void ShiftActivePositionToTheRight()
        {
            if (!_currentPosition.AtEndOfLine())
            {
                _currentPosition = new SelectionPosition(_currentPosition.Line, _currentPosition.ColumnNumber + 1);
            }
        }
    }
}
