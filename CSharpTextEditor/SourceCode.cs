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
        public const string TAB_REPLACEMENT = "    ";

        private readonly LinkedList<SourceCodeLine> _lines;

        public SelectionRange SelectionRange { get; }

        public Cursor Head => SelectionRange.Head;

        public string Text
        {
            get => string.Join(Environment.NewLine, Lines);
            set => SetLinesFromText(value);
        }

        public IReadOnlyCollection<string> Lines => _lines.Select(x => x.Text).ToArray();

        public SourceCode()
            :this(string.Empty)
        { 
        }

        public SourceCode(string text)
        {
            _lines = new LinkedList<SourceCodeLine>(new[] { new SourceCodeLine(string.Empty) });
            if (!string.IsNullOrEmpty(text))
            {
                SetLinesFromText(text);
            }
            if (_lines.Last == null)
            {
                throw new Exception("Something has gone wrong. _lines should always have at least one value here");
            }
            SelectionRange = new SelectionRange(_lines.Last, _lines.Count - 1, _lines.Last.Value.Text.Length);
        }

        private void SetLinesFromText(string text)
        {
            _lines.Clear();
            foreach (string textLine in text.Replace("\t", SourceCode.TAB_REPLACEMENT).Split(Environment.NewLine))
            {
                _lines.AddLast(new SourceCodeLine(textLine));
            }
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            SelectionRange.CancelSelection();
            Cursor position = GetPosition(lineNumber, columnNumber);
            SelectionRange.UpdateHead(position.Line, position.LineNumber, position.ColumnNumber);
        }

        private Cursor GetPosition(int lineNumber, int columnNumber)
        {
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == lineNumber)
                {
                    return new Cursor(current, Math.Min(columnNumber, current.Value.Text.Length), lineNumber);
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                return new Cursor(_lines.Last, Math.Min(columnNumber, _lines.Last.Value.Text.Length), _lines.Count - 1);
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

            Cursor start = GetPosition(startLine, startColumn);
            Cursor end = GetPosition(endLine, endColumn);
            SelectionRange.SelectRange(start, end);
        }

        public void SelectTokenAtPosition(SourceCodePosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            int previousStart = 0;
            Cursor selectionPosition = GetPosition(position.LineNumber, position.ColumnNumber);
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
                SelectionRange.SelectRange(new Cursor(_lines.First, 0, 0), new Cursor(_lines.Last, _lines.Last.Value.Text.Length, _lines.Count - 1));
            }
        }

        internal bool IsRangeSelected()
        {
            return SelectionRange.IsRangeSelected();
        }

        internal void RemoveSelectedRange()
        {
            SelectionRange.RemoveSelected();
        }

        internal void InsertStringAtActivePosition(string v)
        {
            SelectionRange.InsertStringAtActivePosition(v, this, null);
        }

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRange.ShiftHeadOneWordToTheLeft(syntaxHighlighter, shift);
        }

        internal void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRange.ShiftHeadOneWordToTheRight(syntaxHighlighter, shift);
        }

        internal void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRange.RemoveWordBeforeActivePosition(syntaxHighlighter);
        }

        internal void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRange.RemoveWordAfterActivePosition(syntaxHighlighter);
        }

        internal void InsertCharacterAtActivePosition(char keyChar, ISpecialCharacterHandler specialCharacterHandler)
        {
            SelectionRange.InsertCharacterAtActivePosition(keyChar, this, specialCharacterHandler);
        }

        internal void RemoveCharacterBeforeActivePosition()
        {
            SelectionRange.RemoveCharacterBeforeHead();
        }

        internal void RemoveCharacterAfterActivePosition()
        {
            SelectionRange.RemoveCharacterAfterActivePosition();
        }

        internal void InsertLineBreakAtActivePosition(ISpecialCharacterHandler specialCharacterHandler)
        {
            SelectionRange.InsertLineBreakAtActivePosition(this, specialCharacterHandler);
        }

        internal void DecreaseIndentOnSelectedLines()
        {
            SelectionRange.DecreaseIndentOnSelectedLines();
        }

        internal void IncreaseIndentOnSelectedLines()
        {
            SelectionRange.IncreaseIndentOnSelectedLines();
        }

        internal void RemoveTabFromBeforeActivePosition()
        {
            SelectionRange.RemoveTabFromBeforeActivePosition();
        }
    }
}
