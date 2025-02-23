﻿using CSharpTextEditor.UndoRedoActions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SourceCode
    {
        public const string TAB_REPLACEMENT = "    ";

        private readonly LinkedList<SourceCodeLine> _lines;
        private readonly HistoryManager historyManager;

        public SelectionRangeCollection SelectionRangeCollection { get; }

        public string Text
        {
            get => string.Join(Environment.NewLine, Lines);
            set => SetLinesFromText(value);
        }

        public IEnumerable<string> Lines => _lines.Select(x => x.Text);

        public int LineCount => _lines.Count;

        public SourceCode(string text, HistoryManager historyManager)
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
            SelectionRangeCollection = new SelectionRangeCollection(_lines.Last, _lines.Count - 1, _lines.Last.Value.Text.Length);
            this.historyManager = historyManager;
        }

        public void Undo()
        {
            historyManager.Undo(this);
        }

        public void Redo()
        {
            historyManager.Redo(this);
        }

        private void SetLinesFromText(string text)
        {
            _lines.Clear();
            foreach (string textLine in GetLinesFromText(text.Replace("\t", SourceCode.TAB_REPLACEMENT)))
            {
                _lines.AddLast(new SourceCodeLine(textLine));
            }
        }

        private IEnumerable<string> GetLinesFromText(string text)
        {
            using (StringReader sr = new StringReader(text))
            {
                string? current;
                do
                {
                    current = sr.ReadLine();
                    if (current != null)
                    {
                        yield return current;
                    }
                }
                while (current != null);
            }
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            Cursor position = GetCursor(lineNumber, columnNumber);
            SelectionRangeCollection.SetPrimaryActivePosition(position);
        }

        internal Cursor GetCursor(int lineNumber, int columnNumber)
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

        public void ColumnSelect(int startLine, int startColumn, int endLine, int endColumn)
        {
            SelectionRangeCollection.SetSelectionRanges(GetRanges(startLine, startColumn, endLine, endColumn));
        }

        private IEnumerable<(Cursor start, Cursor end)> GetRanges(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine > endLine)
            {
                (startLine, endLine) = (endLine, startLine);
            }
            Cursor current = GetCursor(startLine, startColumn);
            while (current.LineNumber <= endLine)
            {
                Cursor start = new Cursor(current.Line, startColumn, current.LineNumber);
                Cursor end = new Cursor(current.Line, endColumn, current.LineNumber);
                yield return (start, end);
                if (current.Line.Next != null)
                {
                    current.ShiftDownOneLine();
                }
                else
                {
                    break;
                }
            }
        }

        public void SelectRange(SourceCodePosition start, SourceCodePosition end)
        {
            SelectRange(start.LineNumber, start.ColumnNumber, end.LineNumber, end.ColumnNumber);
        }

        public void SelectRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine == endLine
                && startColumn == endColumn)
            {
                SetActivePosition(endLine, endColumn);
                return;
            }

            Cursor start = GetCursor(startLine, startColumn);
            Cursor end = GetCursor(endLine, endColumn);
            SelectionRangeCollection.SetPrimaryRange(start, end);
        }

        public void SelectTokenAtPosition(SourceCodePosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            int previousStart = 0;
            Cursor selectionPosition = GetCursor(position.LineNumber, position.ColumnNumber);
            foreach ((int tStart, int tEnd) in syntaxHighlighter.GetSymbolSpansAfterPosition(selectionPosition.GetPosition().ToCharacterIndex(Lines)))
            {
                int tokenStart = SourceCodePosition.FromCharacterIndex(tStart, Lines).ColumnNumber;
                int tokenEnd = SourceCodePosition.FromCharacterIndex(tEnd, Lines).ColumnNumber;
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
                SelectionRangeCollection.SetPrimaryRange(new Cursor(_lines.First, 0, 0), new Cursor(_lines.Last, _lines.Last.Value.Text.Length, _lines.Count - 1));
            }
        }

        public string GetSelectedText()
        {
            return string.Join(Environment.NewLine, SelectionRangeCollection.Select(x => x.GetSelectedText()));
        }

        internal void RemoveRange(Cursor start, Cursor end)
        {
            SelectionRange range = new SelectionRange(start, end);
            range.RemoveSelected(null);
        }

        internal void RemoveSelectedRange()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveSelected(l), historyManager, "Selection removed");
        }

        internal void InsertStringAtActivePosition(string v)
        {
            string[] lines = GetLinesFromText(v.Replace("\t", SourceCode.TAB_REPLACEMENT)).ToArray();
            if (lines.Length > 1
                && SelectionRangeCollection.Count == lines.Length)
            {
                // special case: the number of lines in the inserted string is equal to the number of carets
                // that means we can insert each line at the corresponding caret, rather than inserting the whole string at each caret
                foreach ((string line, SelectionRange caret) in lines.Zip(SelectionRangeCollection, (x, y) => (x, y)))
                {
                    // TODO Handle undo actions
                    caret.InsertStringAtActivePosition(line, this, null, null);
                }
            }
            else
            {
                SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.InsertStringAtActivePosition(v, this, null, l), historyManager, "Text inserted");
            }
        }

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheLeft(syntaxHighlighter, shift));
        }

        internal void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheRight(syntaxHighlighter, shift));
        }

        internal void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveWordBeforeActivePosition(syntaxHighlighter, l), historyManager, "Word removed");
        }

        internal void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveWordAfterActivePosition(syntaxHighlighter, l), historyManager, "Word removed");
        }

        internal void InsertCharacterAtActivePosition(char keyChar, ISpecialCharacterHandler specialCharacterHandler)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.InsertCharacterAtActivePosition(keyChar, this, specialCharacterHandler, l), historyManager, "Character inserted");
        }

        internal void RemoveCharacterBeforeActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveCharacterBeforeHead(l), historyManager, "Character removed");
        }

        internal void RemoveCharacterAfterActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveCharacterAfterActivePosition(l), historyManager, "Character removed");
        }

        internal void InsertLineBreakAtActivePosition(ISpecialCharacterHandler specialCharacterHandler)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.InsertLineBreakAtActivePosition(this, l, specialCharacterHandler), historyManager, "Line break inserted");
        }

        internal void DecreaseIndentOnSelectedLines()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.DecreaseIndentOnSelectedLines(l), historyManager, "Indent decreased");
        }

        internal void IncreaseIndentOnSelectedLines()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.IncreaseIndentOnSelectedLines(l), historyManager, "Indent increased");
        }

        internal void RemoveTabFromBeforeActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveTabFromBeforeActivePosition(l), historyManager, "Indent decreased");
        }

        internal void ShiftHeadToTheLeft(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToTheLeft(selection));
        }

        internal void ShiftHeadToTheRight(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToTheRight(selection));
        }

        internal void ShiftHeadUpOneLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadUpOneLine(selection));
        }

        internal void ShiftHeadDownOneLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadDownOneLine(selection));
        }

        internal void ShiftHeadToEndOfLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToEndOfLine(selection));
        }

        internal void ShiftHeadToStartOfLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToHome(selection));
        }

        internal void ShiftHeadUpLines(int v, bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadUpLines(v, selection));
        }

        internal void ShiftHeadDownLines(int v, bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadDownLines(v, selection));
        }

        internal bool SelectionCoversMultipleLines()
        {
            return SelectionRangeCollection.PrimarySelectionRange.SelectionCoversMultipleLines();
        }

        internal void IncreaseIndentAtActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.IncreaseIndentAtActivePosition(l), historyManager, "Indent increased");
        }

        internal void DecreaseIndentAtActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.DecreaseIndentAtActivePosition(l), historyManager, "Indent decreased");
        }

        internal void DuplicateSelection()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.DuplicateSelection(this, l), historyManager, "Selection duplicated");
        }
    }
}
