using CSharpTextEditor.Languages;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.Utility;
using System;
using System.Collections.Generic;
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
        private readonly HistoryManager _historyManager;
        private readonly ISourceCodeListener? _sourceCodeListener;

        public SelectionRangeCollection SelectionRangeCollection { get; }

        /// <summary>
        /// If true, typing a character will overwrite the character at the current position,
        /// otherwise the character will be inserted at the current position
        /// </summary>
        public bool OvertypeEnabled { get; set; } = false;

        public string Text
        {
            get => string.Join(Environment.NewLine, Lines);
            set => SetLinesFromText(value);
        }

        public IEnumerable<string> Lines => _lines.Select(x => x.Value.Text);

        public int LineCount => _lines.Count;

        public SourceCode(string text)
            : this(text, new HistoryManager(), null!)
        {
        }

        public SourceCode(string text, HistoryManager historyManager, ISourceCodeListener sourceCodeListener)
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
            this._historyManager = historyManager;
            this._sourceCodeListener = sourceCodeListener;
        }

        public void Undo()
        {
            _historyManager.Undo(this);
            _sourceCodeListener?.TextChanged();
            _sourceCodeListener?.CursorsChanged();
        }

        public void Redo()
        {
            _historyManager.Redo(this);
            _sourceCodeListener?.TextChanged();
            _sourceCodeListener?.CursorsChanged();
        }

        private void SetLinesFromText(string text)
        {
            _lines.Clear();
            foreach (string textLine in text.Replace("\t", TAB_REPLACEMENT).SplitIntoLines())
            {
                _lines.AddLast(new SourceCodeLine(textLine));
            }
            _sourceCodeListener?.TextChanged();
        }

        public int AddCaret(int lineNumber, int columnNumber)
        {
            Cursor position = GetCursor(lineNumber, columnNumber);
            SelectionRangeCollection.AddSelectionRange(null, position);
            _sourceCodeListener?.CursorsChanged();
            return SelectionRangeCollection.Count - 1;
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            Cursor position = GetCursor(lineNumber, columnNumber);
            SelectionRangeCollection.SetPrimaryActivePosition(position);
            _sourceCodeListener?.CursorsChanged();
        }

        public void SelectRanges(IEnumerable<(SourceCodePosition? start, SourceCodePosition end)> ranges)
        {
            SelectionRangeCollection.SetSelectionRanges(ranges.Select(x => (x.start != null ? GetCursor(x.start.Value) : null, GetCursor(x.end))));
            _sourceCodeListener?.CursorsChanged();
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
            _sourceCodeListener?.CursorsChanged();
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
                _sourceCodeListener?.CursorsChanged();
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
            _sourceCodeListener?.CursorsChanged();
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
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.RemoveSelectedRange(l), _historyManager, "Selection removed", _sourceCodeListener);
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
                    _historyManager.AddAction(builder.Build("Text inserted"));
                }
            }
            else
            {
                SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.InsertStringAtActivePosition(v, this, l, null), _historyManager, "Text inserted", _sourceCodeListener);
            }
        }

        internal void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) =>
                {
                    var result = r.RemoveWordBeforeActivePosition(syntaxHighlighter, l);
                    syntaxHighlighter.Update(Lines);
                    return result;
                },
            _historyManager, "Word removed", _sourceCodeListener);
        }

        internal void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) =>
                {
                    var result = r.RemoveWordAfterActivePosition(syntaxHighlighter, l);
                    syntaxHighlighter.Update(Lines);
                    return result;
                },
                _historyManager, "Word removed", _sourceCodeListener);
        }

        internal void RemoveCharacterBeforeActivePosition()
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.RemoveCharacterBeforeActivePosition(l), _historyManager, "Character removed", _sourceCodeListener);
        }

        internal void RemoveCharacterAfterActivePosition()
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.RemoveCharacterAfterActivePosition(l), _historyManager, "Character removed", _sourceCodeListener);
        }

        internal void RemoveLineAtActivePosition()
        {
            // Don't use DoEditActionOnAllRanges here, as each SelectionRange is too dependent on the others
            HistoryActionBuilder builder = new HistoryActionBuilder();

            List<SelectionRange> ordered = SelectionRangeCollection.OrderBy(x => x.Head).ToList();
            Queue<SourceCodePosition?> originalTailPositions =  new Queue<SourceCodePosition?>(ordered.Select(x => x.Tail?.GetPosition()));
            Queue<SourceCodePosition> originalHeadPositions = new Queue<SourceCodePosition>(ordered.Select(x => x.Head.GetPosition()));

            var groupedRanges = GroupRangesByCommonLines(ordered).ToList();

            foreach ((List<SelectionRange> ranges, IReadOnlyCollection<int> lineIndices) in groupedRanges)
            {
                List<UndoRedoAction> actions = new List<UndoRedoAction>();

                // if there are multiple ranges on the same line, after the action there will only be one range left
                // do all the actual operations just using the first range
                var range = ranges.First();
                Cursor current;
                int linesToRemove = 1 + (lineIndices.Max() - lineIndices.Min());
                if (range.IsRangeSelected())
                {
                    (current, var end) = range.GetOrderedCursors();
                }
                else
                {
                    current = range.Head;
                }

                range.Head.CopyFrom(current);
                range.CancelSelection();

                while (linesToRemove-- > 0)
                {
                    range.Head.ShiftToEndOfLine();
                    range.SelectRange(GetCursor(range.Head.LineNumber, 0), range.Head);
                    range.Head.ShiftOneCharacterToTheRight();
                    range.RemoveSelectedRange(actions);
                }
                builder.Add(new SelectionRangeActionList(actions, originalTailPositions.Dequeue(), null, originalHeadPositions.Dequeue(), range.Head.GetPosition()));
                foreach (var r in ranges.Skip(1))
                {
                    // add an empty action list. (This range will no longer exist after the action, so we just need to record the original positions)
                    builder.Add(new SelectionRangeActionList([], originalTailPositions.Dequeue(), null, originalHeadPositions.Dequeue(), null));

                    // set this range to be the same as the first, so it gets cleared later
                    r.CancelSelection();
                    r.Head.CopyFrom(range.Head);
                }
            }
            SelectionRangeCollection.ResolveOverlappingRanges();
            _sourceCodeListener?.TextChanged();
            _sourceCodeListener?.CursorsChanged();
            _historyManager.AddAction(builder.Build("Line removed"));
        }

        internal void SwapLinesUpAtActivePosition()
        {
            if (SelectionRangeCollection.Any(x => x.ContainsLine(0)))
            {
                return;
            }
            List<SelectionRange> ordered = SelectionRangeCollection.OrderBy(x => x.Head).ToList();

            Queue<SourceCodePosition?> originalTailPositions = new Queue<SourceCodePosition?>(ordered.Select(x => x.Tail?.GetPosition()));
            Queue<SourceCodePosition> originalHeadPositions = new Queue<SourceCodePosition>(ordered.Select(x => x.Head.GetPosition()));
            Queue<List<int>> originalLinesCovered = new Queue<List<int>>(ordered.Select(x => x.GetContainedLines().ToList()));

            HashSet<int> linesCovered = new HashSet<int>();
            HistoryActionBuilder builder = new HistoryActionBuilder();
            List<UndoRedoAction> actions = new List<UndoRedoAction>();
            List<SelectionRangeActionList> actionLists = new List<SelectionRangeActionList>();
            bool first = true;
            foreach (SelectionRange range in ordered)
            {
                SourceCodePosition? originalTail = originalTailPositions.Dequeue();
                SourceCodePosition originalHead = originalHeadPositions.Dequeue();
                IReadOnlyList<int> originalLines = originalLinesCovered.Dequeue();

                foreach (int lineIndex in originalLines)
                {
                    if (linesCovered.Add(lineIndex))
                    {
                        Cursor cursor = GetCursor(lineIndex, 0);
                        _lines.SwapWithPrevious(cursor.Line);
                        actions.Add(new LineSwapAction(new SourceCodePosition(lineIndex, 0), new SourceCodePosition(lineIndex - 1, 0), false));
                    }
                }
                actionLists.Add(new SelectionRangeActionList(first ? actions : [], originalTail, range.Tail?.GetPosition(), originalHead, range.Head.GetPosition()));
                first = false;
            }
            foreach (var actionList in actionLists)
            {
                builder.Add(actionList);
            }
            SelectionRangeCollection.ResolveOverlappingRanges();
            _sourceCodeListener?.TextChanged();
            _sourceCodeListener?.CursorsChanged();
            _historyManager.AddAction(builder.Build("Lines swapped"));
        }

        internal void SwapLinesDownAtActivePosition()
        {
            if (SelectionRangeCollection.Any(x => x.ContainsLine(LineCount - 1)))
            {
                return;
            }
            List<SelectionRange> ordered = SelectionRangeCollection.OrderByDescending(x => x.Head).ToList();

            Queue<SourceCodePosition?> originalTailPositions = new Queue<SourceCodePosition?>(ordered.Select(x => x.Tail?.GetPosition()));
            Queue<SourceCodePosition> originalHeadPositions = new Queue<SourceCodePosition>(ordered.Select(x => x.Head.GetPosition()));
            Queue<List<int>> originalLinesCovered = new Queue<List<int>>(ordered.Select(x => x.GetContainedLines().ToList()));

            HashSet<int> linesCovered = new HashSet<int>();
            HistoryActionBuilder builder = new HistoryActionBuilder();
            List<UndoRedoAction> actions = new List<UndoRedoAction>();
            List<SelectionRangeActionList> actionLists = new List<SelectionRangeActionList>();
            bool first = true;
            foreach (SelectionRange range in ordered)
            {
                SourceCodePosition? originalTail = originalTailPositions.Dequeue();
                SourceCodePosition originalHead = originalHeadPositions.Dequeue();
                IReadOnlyList<int> originalLines = originalLinesCovered.Dequeue();

                foreach (int lineIndex in originalLines.Reverse())
                {
                    if (linesCovered.Add(lineIndex))
                    {
                        Cursor cursor = GetCursor(lineIndex, 0);
                        _lines.SwapWithNext(cursor.Line);
                        actions.Add(new LineSwapAction(new SourceCodePosition(lineIndex, 0), new SourceCodePosition(lineIndex + 1, 0), true));
                    }
                }
                actionLists.Add(new SelectionRangeActionList(first ? actions : [], originalTail, range.Tail?.GetPosition(), originalHead, range.Head.GetPosition()));
                first = false;
            }
            foreach (var actionList in Enumerable.Reverse(actionLists))
            {
                builder.Add(actionList);
            }
            SelectionRangeCollection.ResolveOverlappingRanges();
            _sourceCodeListener?.TextChanged();
            _sourceCodeListener?.CursorsChanged();
            _historyManager.AddAction(builder.Build("Lines swapped"));
        }

        private IEnumerable<(List<SelectionRange>, IReadOnlyCollection<int>)> GroupRangesByCommonLines(List<SelectionRange> orderedRanges)
        {
            HashSet<int> lineIndices = new HashSet<int> { orderedRanges[0].Head.LineNumber };
            if (orderedRanges[0].Tail != null)
            {
                lineIndices.Add(orderedRanges[0].Tail.LineNumber);
            }
            List<SelectionRange> currentRanges = new List<SelectionRange> { orderedRanges[0] };
            
            foreach (var range in orderedRanges.Skip(1))
            {
                bool addTail = range.Tail == null || !lineIndices.Contains(range.Tail.LineNumber);
                bool addHead = !lineIndices.Contains(range.Head.LineNumber);
                if (!addTail || !addHead)
                {
                    if (range.Tail != null)
                    {
                        lineIndices.Add(range.Tail.LineNumber);
                    }
                    lineIndices.Add(range.Head.LineNumber);
                    currentRanges.Add(range);
                }
                else
                {
                    yield return (currentRanges, lineIndices);
                    currentRanges = new List<SelectionRange> { range };
                    lineIndices = new HashSet<int> { range.Head.LineNumber };
                    if (range.Tail != null)
                    {
                        lineIndices.Add(range.Tail.LineNumber);
                    }
                }
            }
            yield return (currentRanges, lineIndices);
        }

        internal void InsertCharacterAtActivePosition(char keyChar, ISpecialCharacterHandler? specialCharacterHandler)
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.InsertCharacterAtActivePosition(keyChar, this, l, specialCharacterHandler, OvertypeEnabled), _historyManager, "Character inserted", _sourceCodeListener);
        }

        internal void InsertLineBreakAtActivePosition(ISpecialCharacterHandler? specialCharacterHandler)
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.InsertLineBreakAtActivePosition(this, l, specialCharacterHandler), _historyManager, "Line break inserted", _sourceCodeListener);
        }

        internal void ShiftHeadToTheLeft(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToTheLeft(selection), _sourceCodeListener);
        }

        internal void ShiftHeadToTheRight(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToTheRight(selection), _sourceCodeListener);
        }

        internal void ShiftHeadUpOneLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadUpOneLine(selection), _sourceCodeListener);
        }

        internal void ShiftHeadDownOneLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadDownOneLine(selection), _sourceCodeListener);
        }

        internal void ShiftHeadToEndOfLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToEndOfLine(selection), _sourceCodeListener);
        }

        internal void ShiftHeadToStartOfLine(bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadToHome(selection), _sourceCodeListener);
        }

        internal void ShiftHeadUpLines(int v, bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadUpLines(v, selection), _sourceCodeListener);
        }

        internal void ShiftHeadDownLines(int v, bool selection)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadDownLines(v, selection), _sourceCodeListener);
        }

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheLeft(syntaxHighlighter, shift), _sourceCodeListener);
        }

        internal void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool shift)
        {
            SelectionRangeCollection.DoActionOnAllRanges((r) => r.ShiftHeadOneWordToTheRight(syntaxHighlighter, shift), _sourceCodeListener);
        }

        internal bool SelectionCoversMultipleLines()
        {
            return SelectionRangeCollection.PrimarySelectionRange.SelectionCoversMultipleLines();
        }

        internal void IncreaseIndentAtActivePosition()
        {
            HashSet<int> linesToIgnore = new HashSet<int>();
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.IncreaseIndentAtActivePosition(l, linesToIgnore), _historyManager, "Indent increased", _sourceCodeListener);
        }

        internal void DecreaseIndentAtActivePosition()
        {
            HashSet<int> linesToIgnore = new HashSet<int>();
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.DecreaseIndentAtActivePosition(l, linesToIgnore), _historyManager, "Indent decreased", _sourceCodeListener);
        }

        internal void DuplicateSelection()
        {
        SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.DuplicateSelection(this, l), _historyManager, "Selection duplicated", _sourceCodeListener);
        }

        internal void SelectionToLowerCase()
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.SelectionToLowerCase(this, l), _historyManager, "Make lowercase", _sourceCodeListener);
        }

        internal void SelectionToUpperCase()
        {
            SelectionRangeCollection.DoEditActionOnAllRanges((r, l) => r.SelectionToUpperCase(this, l), _historyManager, "Make uppercase", _sourceCodeListener);
        }
    }
}
