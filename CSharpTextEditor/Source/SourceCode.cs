using CSharpTextEditor.Languages;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Source
{
    internal class SourceCode
    {
        public const string TAB_REPLACEMENT = "    ";

        private readonly SourceCodeLineList _lines;
        private readonly HistoryManager historyManager;

        public SelectionRangeCollection SelectionRangeCollection { get; }

        public string Text
        {
            get => string.Join(Environment.NewLine, Lines);
            set => SetLinesFromText(value);
        }

        public IEnumerable<string> Lines => _lines.Select(x => x.Value.Text);

        public int LineCount => _lines.Count;

        public SourceCode(string text, HistoryManager historyManager)
        {
            _lines = new SourceCodeLineList(new[] { new SourceCodeLine(string.Empty) });
            if (!string.IsNullOrEmpty(text))
            {
                SetLinesFromText(text);
            }
            if (_lines.Last == null)
            {
                throw new Exception("Something has gone wrong. _lines should always have at least one value here");
            }
            SelectionRangeCollection = new SelectionRangeCollection(_lines.Last, _lines.Last.Value.Text.Length);
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
            foreach (string textLine in text.Replace("\t", TAB_REPLACEMENT).SplitIntoLines())
            {
                _lines.AddLast(new SourceCodeLine(textLine));
            }
        }

        public int AddCaret(int lineNumber, int columnNumber)
        {
            Cursor position = GetCursor(lineNumber, columnNumber);
            SelectionRangeCollection.AddSelectionRange(null, position);
            return SelectionRangeCollection.Count - 1;
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            Cursor position = GetCursor(lineNumber, columnNumber);
            SelectionRangeCollection.SetPrimaryActivePosition(position);
        }

        public void SelectRanges(IEnumerable<(SourceCodePosition? start, SourceCodePosition end)> ranges)
        {
            SelectionRangeCollection.SetSelectionRanges(ranges.Select(x => (x.start != null ? GetCursor(x.start.Value) : null, GetCursor(x.end))));
        }

        internal Cursor GetCursor(int lineNumber, int columnNumber) => GetCursor(new SourceCodePosition(lineNumber, columnNumber)); 

        internal Cursor GetCursor(SourceCodePosition position)
        {
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == position.LineNumber)
                {
                    return new Cursor(current, Math.Min(position.ColumnNumber, current.Value.Text.Length));
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                return new Cursor(_lines.Last, Math.Min(position.ColumnNumber, _lines.Last.Value.Text.Length));
            }
            throw new CSharpTextEditorException("Couldn't get position");
        }

        public void ColumnSelect(int startLine, int startColumn, int endLine, int endColumn)
        {
            SelectionRangeCollection.SetSelectionRanges(GetRanges(startLine, startColumn, endLine, endColumn));
        }

        private IEnumerable<(Cursor? start, Cursor end)> GetRanges(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine > endLine)
            {
                (startLine, endLine) = (endLine, startLine);
            }
            Cursor current = GetCursor(startLine, startColumn);
            while (current.LineNumber <= endLine)
            {
                Cursor start = new Cursor(current.Line, startColumn);
                Cursor end = new Cursor(current.Line, endColumn);
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

        public void SelectRange(int startLine, int startColumn, int endLine, int endColumn, int caretIndex = SelectionRangeCollection.PRIMARY_INDEX)
        {
            if (startLine == endLine
                && startColumn == endColumn)
            {
                if (caretIndex == SelectionRangeCollection.PRIMARY_INDEX)
                {
                    SetActivePosition(endLine, endColumn);
                }
                else
                {
                    Cursor position = GetCursor(endLine, endColumn);
                    SelectionRangeCollection.SetSelectionRange(caretIndex, null, position);
                }
                return;
            }

            Cursor start = GetCursor(startLine, startColumn);
            Cursor end = GetCursor(endLine, endColumn);
            if (caretIndex == SelectionRangeCollection.PRIMARY_INDEX)
            {
                SelectionRangeCollection.SetPrimarySelectionRange(start, end);
            }
            else
            {
                SelectionRangeCollection.SetSelectionRange(caretIndex, start, end);
            }
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
                SelectionRangeCollection.SetPrimarySelectionRange(new Cursor(_lines.First, 0), new Cursor(_lines.Last, _lines.Last.Value.Text.Length));
            }
        }

        public string GetSelectedText()
        {
            return string.Join(Environment.NewLine, SelectionRangeCollection.Select(x => x.GetSelectedText()));
        }

        internal void RemoveSelectedRange()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveSelectedRange(l), historyManager, "Selection removed");
        }

        internal void InsertStringAtActivePosition(string v)
        {
            string[] lines = v.Replace("\t", TAB_REPLACEMENT).SplitIntoLines().ToArray();
            if (lines.Length > 1
                && SelectionRangeCollection.GetDistinctLineCount() == lines.Length)
            {
                // special case: the number of lines in the inserted string is equal to the number of carets
                // that means we can insert each line at the corresponding caret, rather than inserting the whole string at each caret
                HistoryActionBuilder builder = new HistoryActionBuilder();
                int index = 0;
                foreach ((string line, SelectionRange caret) in lines.Zip(SelectionRangeCollection, (x, y) => (x, y)))
                {
                    var tailBefore = caret.Tail?.GetPosition();
                    var headBefore = caret.Head.GetPosition();
                    List<UndoRedoAction> actions = new List<UndoRedoAction>();
                    caret.InsertStringAtActivePosition(line, this, actions, null);
                    builder.Add(new SelectionRangeActionList(actions, tailBefore, caret.Tail?.GetPosition(), headBefore, caret.Head.GetPosition()));
                    index++;
                }
                if (builder.Any())
                {
                    historyManager.AddAction(builder.Build("Text inserted"));
                }
            }
            else
            {
                SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.InsertStringAtActivePosition(v, this, l, null), historyManager, "Text inserted");
            }
        }

        internal void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveWordBeforeActivePosition(syntaxHighlighter, l), historyManager, "Word removed");
        }

        internal void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveWordAfterActivePosition(syntaxHighlighter, l), historyManager, "Word removed");
        }

        internal void RemoveCharacterBeforeActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveCharacterBeforeActivePosition(l), historyManager, "Character removed");
        }

        internal void RemoveCharacterAfterActivePosition()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.RemoveCharacterAfterActivePosition(l), historyManager, "Character removed");
        }

        internal void InsertCharacterAtActivePosition(char keyChar, ISpecialCharacterHandler? specialCharacterHandler)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.InsertCharacterAtActivePosition(keyChar, this, l, specialCharacterHandler), historyManager, "Character inserted");
        }

        internal void InsertLineBreakAtActivePosition(ISpecialCharacterHandler? specialCharacterHandler)
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

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheLeft(syntaxHighlighter, shift));
        }

        internal void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheRight(syntaxHighlighter, shift));
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

        internal void SelectionToLowerCase()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.SelectionToLowerCase(this, l), historyManager, "Make lowercase");
        }

        internal void SelectionToUpperCase()
        {
            SelectionRangeCollection.DoActionOnAllRanges((r, l) => r.SelectionToUpperCase(this, l), historyManager, "Make uppercase");
        }
    }
}
