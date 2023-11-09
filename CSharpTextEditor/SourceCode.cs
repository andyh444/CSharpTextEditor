﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SourceCode
    {
        public const string TAB_REPLACEMENT = "    ";

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
                _lines = new LinkedList<string>(value.Replace("\t", SourceCode.TAB_REPLACEMENT).Split(Environment.NewLine));
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
            RemoveRange(_selectionStart ?? _selectionEnd, _selectionEnd);
        }

        private void RemoveRange(SelectionPosition start, SelectionPosition end)
        {
            (SelectionPosition first, SelectionPosition last) = GetFirstAndLastSelectionPositions(start, end);
            while (last > first)
            {
                RemoveCharacterBeforePosition(last);
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
            return GetFirstAndLastSelectionPositions(_selectionStart ?? _selectionEnd, _selectionEnd);
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

        public void RemoveTabFromBeforeActivePosition() => RemoveTabFromBeforePosition(_selectionEnd);

        private void RemoveTabFromBeforePosition(SelectionPosition position)
        {
            // TODO: Different behaviour if there is a range selected
            if (position.AtStartOfLine())
            {
                return;
            }
            string textBeforePosition = position.GetLineValue().Substring(0, position.ColumnNumber);
            if (string.IsNullOrWhiteSpace(textBeforePosition))
            {
                SelectionPosition otherEnd = position.Clone();
                otherEnd.ColumnNumber = Math.Max(0, otherEnd.ColumnNumber - TAB_REPLACEMENT.Length);
                RemoveRange(otherEnd, position);
            }
        }

        private void RemoveCharacterBeforePosition(SelectionPosition position)
        {
            if (position.ColumnNumber > 0)
            {
                if (position.AtEndOfLine())
                {
                    position.Line.Value = position.GetLineValue().Substring(0, position.ColumnNumber - 1);
                    position.ColumnNumber--;
                }
                else if (position.ColumnNumber == 1)
                {
                    position.Line.Value = position.GetLineValue().Substring(1);
                    position.ColumnNumber--;
                }
                else
                {
                    string before = position.GetLineValue().Substring(0, position.ColumnNumber - 1);
                    string after = position.GetLineValue().Substring(position.ColumnNumber);
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
            _selectionEnd.ResetMaxColumnNumber();
        }

        public void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            RemoveWordBeforePosition(_selectionEnd, syntaxHighlighter);
        }

        private void RemoveWordBeforePosition(SelectionPosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionPosition startOfPreviousWord = position.Clone();
            startOfPreviousWord.ShiftOneWordToTheLeft(syntaxHighlighter);
            RemoveRange(startOfPreviousWord, position);
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
                    string before = _selectionEnd.GetLineValue().Substring(0, _selectionEnd.ColumnNumber);
                    string after = _selectionEnd.GetLineValue().Substring(_selectionEnd.ColumnNumber + 1);
                    _selectionEnd.Line.Value = before + after;
                }
                else if (_selectionEnd.Line.Next != null)
                {
                    _selectionEnd.Line.Value += _selectionEnd.Line.Next.Value;
                    _lines.Remove(_selectionEnd.Line.Next);
                }
            }
            _selectionEnd.ResetMaxColumnNumber();
        }

        public void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            RemoveWordAfterPosition(_selectionEnd, syntaxHighlighter);
        }

        private void RemoveWordAfterPosition(SelectionPosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionPosition startOfNextWord = position.Clone();
            startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
            RemoveRange(position, startOfNextWord);
        }

        public void IncreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (SelectionPosition start, SelectionPosition end) = GetFirstAndLastSelectionPositions(_selectionStart.Clone(), _selectionEnd.Clone());
                while (start.LineNumber <= end.LineNumber)
                {
                    start.Line.Value = TAB_REPLACEMENT + start.GetLineValue();
                    start.ShiftDownOneLine();
                }
                _selectionStart.ColumnNumber += TAB_REPLACEMENT.Length;
                _selectionEnd.ColumnNumber += TAB_REPLACEMENT.Length;
            }
        }

        public void DecreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (SelectionPosition start, SelectionPosition end) = GetFirstAndLastSelectionPositions(_selectionStart.Clone(), _selectionEnd.Clone());
                while (start.LineNumber <= end.LineNumber)
                {
                    string lineValue = start.GetLineValue();
                    if (lineValue.Length < TAB_REPLACEMENT.Length)
                    {
                        int firstNonWhiteSpaceCharacter = 0;
                        for (int i = 0; i < lineValue.Length; i++)
                        {
                            if (!char.IsWhiteSpace(lineValue[i]))
                            {
                                firstNonWhiteSpaceCharacter = i;
                                break;
                            }
                        }
                        if (firstNonWhiteSpaceCharacter > 0)
                        {
                            start.Line.Value = lineValue.Substring(firstNonWhiteSpaceCharacter);
                        }
                    }
                    else if (lineValue.Substring(0, TAB_REPLACEMENT.Length) == TAB_REPLACEMENT)
                    {
                        start.Line.Value = lineValue.Substring(TAB_REPLACEMENT.Length);
                    }
                    start.ShiftDownOneLine();
                }
                _selectionStart.ColumnNumber -= Math.Max(0, TAB_REPLACEMENT.Length);
                _selectionEnd.ColumnNumber -= Math.Max(0, TAB_REPLACEMENT.Length);
            }
        }

        public void InsertLineBreakAtActivePosition(ISpecialCharacterHandler? specialCharacterHandler = null)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            string newLineContents = string.Empty;
            if (!_selectionEnd.AtEndOfLine())
            {
                newLineContents = _selectionEnd.GetLineValue().Substring(_selectionEnd.ColumnNumber);
                _selectionEnd.Line.Value = _selectionEnd.GetLineValue().Substring(0, _selectionEnd.ColumnNumber);
            }
            LinkedListNode<string> newLine = _lines.AddAfter(_selectionEnd.Line, newLineContents);
            _selectionEnd = new SelectionPosition(newLine, 0, _selectionEnd.LineNumber + 1);
            _selectionEnd.ResetMaxColumnNumber();

            specialCharacterHandler?.HandleLineBreakInserted(this, _selectionEnd);
        }

        public void InsertCharacterAtActivePosition(char character, ISpecialCharacterHandler? specialCharacterHandler)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            if (character == '\t')
            {
                InsertStringAtActivePosition(TAB_REPLACEMENT);
                return;
            }
            specialCharacterHandler?.HandleCharacterInserting(character, this);
            if (_selectionEnd.AtEndOfLine())
            {
                _selectionEnd.Line.Value = _selectionEnd.Line.Value + character;
                _selectionEnd.ColumnNumber++;
            }
            else
            {

                _selectionEnd.Line.Value = string.Concat(
                    _selectionEnd.GetLineValue().Substring(0, _selectionEnd.ColumnNumber),
                    character,
                    _selectionEnd.GetLineValue().Substring(_selectionEnd.ColumnNumber));
                _selectionEnd.ColumnNumber++;
            }
            _selectionEnd.ResetMaxColumnNumber();
        }

        public void InsertStringAtActivePosition(string text)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            text = text.Replace("\t", SourceCode.TAB_REPLACEMENT);
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
                            _selectionEnd.GetLineValue().Substring(0, _selectionEnd.ColumnNumber),
                            currentLine,
                            _selectionEnd.GetLineValue().Substring(_selectionEnd.ColumnNumber));
                    }
                    _selectionEnd.ColumnNumber += currentLine.Length;
                    currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        InsertLineBreakAtActivePosition();
                    }
                }
            }
            _selectionEnd.ResetMaxColumnNumber();
        }

        public void ShiftActivePositionUpOneLine(bool selection)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftUpOneLine();
        }

        public void ShiftActivePositionDownOneLine(bool selection)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftDownOneLine();
        }

        public void ShiftActivePositionUpLines(int lineCount, bool selection)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftUpLines(lineCount);
        }

        public void ShiftActivePositionDownLines(int lineCount, bool selection)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftDownLines(lineCount);
        }

        public void ShiftActivePositionToTheLeft(bool selection)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftOneCharacterToTheLeft();
        }

        public void ShiftActivePositionToTheRight(bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftOneCharacterToTheRight();
        }

        public void ShiftActivePositionOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftOneWordToTheRight(syntaxHighlighter);
        }

        internal void ShiftActivePositionOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftOneWordToTheLeft(syntaxHighlighter);
        }

        public void ShiftActivePositionToEndOfLine(bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftToEndOfLine();
        }

        public void ShiftActivePositionToStartOfLine(bool selection = false)
        {
            UpdateSelectionStart(selection);
            _selectionEnd.ShiftToStartOfLine();
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
            _selectionEnd = GetPosition(lineNumber, columnNumber);
        }

        private SelectionPosition GetPosition(int lineNumber, int columnNumber)
        {
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == lineNumber)
                {
                    return new SelectionPosition(current, Math.Min(columnNumber, current.Value.Length), lineNumber);
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                return new SelectionPosition(_lines.Last, Math.Min(columnNumber, _lines.Last.Value.Length), _lines.Count - 1);
            }
            throw new Exception("Couldn't get position");
        }

        public void SelectRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine == endLine
                && startColumn == endColumn)
            {
                SetActivePosition(endLine, endColumn);
                return;
            }
            _selectionStart = GetPosition(startLine, startColumn);
            _selectionEnd = GetPosition(endLine, endColumn);
        }

        public void SelectTokenAtPosition(SourceCodePosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            int previousStart = 0;
            SelectionPosition selectionPosition = GetPosition(position.LineNumber, position.ColumnNumber);
            foreach ((int tokenStart, int tokenEnd) in syntaxHighlighter.GetSpansFromTextLine(selectionPosition.GetLineValue()))
            {
                if (position.ColumnNumber <= tokenStart)
                {
                    SelectRange(position.LineNumber, previousStart, position.LineNumber, tokenStart);
                    break;
                }
                if (tokenStart <= position.ColumnNumber && tokenEnd >= position.ColumnNumber)
                {
                    SelectRange(position.LineNumber, tokenStart, position.LineNumber, tokenEnd);
                    break;
                }
                previousStart = tokenEnd;
            }
        }

        public void SelectAll()
        {
            if (_lines.First != null
                && _lines.Last != null)
            {
                _selectionStart = new SelectionPosition(_lines.First, 0, 0);
                _selectionEnd = new SelectionPosition(_lines.Last, _lines.Last.Value.Length, _lines.Count - 1);
            }
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
                start.ShiftOneCharacterToTheRight();
            }
            return sb.ToString();
        }
    }
}
