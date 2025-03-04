using CSharpTextEditor.UndoRedoActions;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Source
{

    internal class SelectionRangeCollection : IReadOnlyCollection<SelectionRange>
    {
        public const int PRIMARY_INDEX = 0;

        private readonly List<SelectionRange> _selectionRanges;

        public int Count => _selectionRanges.Count;

        public SelectionRange PrimarySelectionRange => _selectionRanges.First();

        public SelectionRangeCollection(ISourceCodeLineNode initialLine, int initialColumnNumber)
        {
            _selectionRanges = new List<SelectionRange> { new SelectionRange(initialLine, initialColumnNumber) };
        }

        public void DoActionOnAllRanges(Action<SelectionRange> action)
        {
            foreach (var range in _selectionRanges.OrderByDescending(x => x.Head))
            {
                action(range);
            }
        }

        public void DoActionOnAllRanges(Action<SelectionRange, List<UndoRedoAction>> action, HistoryManager manager, string displayName)
        {
            HistoryActionBuilder builder = new HistoryActionBuilder();
            int index = 0;// _selectionRanges.Count - 1;

            SourceCodePosition? lastPositionBefore = null;
            SourceCodePosition? lastPositionAfter = null;
            SelectionRange? previous = null;

            List<SelectionRange> ordered = _selectionRanges.OrderBy(x => x.Head).ToList();
            List<SourceCodePosition?> originalTailPositions = ordered.Select(x => x.Tail?.GetPosition()).ToList();
            List<SourceCodePosition> originalHeadPositions = ordered.Select(x => x.Head.GetPosition()).ToList();

            foreach (SelectionRange range in ordered)
            {
                if (lastPositionBefore != null
                    && lastPositionAfter != null
                    && previous != null
                    && range.Head.LineNumber == lastPositionBefore.Value.LineNumber)
                {
                    // this caret is on the same line as the previous caret, therefore the action of the previous caret will affect this one's position
                    int columnDifference = range.Head.ColumnNumber - lastPositionBefore.Value.ColumnNumber;
                    lastPositionBefore = range.Head.GetPosition();
                    int tailDifference = 0;
                    if (range.Tail != null)
                    {
                        tailDifference = range.Tail.GetPositionDifference(range.Head);
                    }
                    range.Head.Line = previous.Head.Line;
                    range.Head.ColumnNumber = lastPositionAfter.Value.ColumnNumber + columnDifference;

                    if (range.Tail != null)
                    {
                        range.Tail.Line = range.Head.Line;
                        range.Tail.ColumnNumber = range.Head.ColumnNumber;
                        range.Tail.ShiftPosition(-tailDifference);
                    }
                }
                else
                {
                    lastPositionBefore = range.Head.GetPosition();
                }

                SelectionRangeActionList list = builder.Add(index);
                list.TailBefore = originalTailPositions[index];
                list.HeadBefore = originalHeadPositions[index];
                action(range, list.UndoRedoActions);
                lastPositionAfter = range.Head.GetPosition();
                list.HeadAfter = lastPositionAfter.Value;
                list.TailAfter = range.Tail?.GetPosition();
                previous = range;
                index++;
            }
            if (builder.Any())
            {
                manager.AddAction(builder.Build(displayName));
            }
        }

        /// <summary>
        /// Returns the count of selection ranges with unique lines
        /// </summary>
        public int GetDistinctLineCount() => new HashSet<int>(_selectionRanges.Select(x => x.Head.LineNumber)).Count;

        public void SetSelectionRanges(IEnumerable<(Cursor? start, Cursor end)> ranges)
        {
            bool primarySet = false;
            foreach ((Cursor? start, Cursor end) in ranges)
            {
                if (!primarySet)
                {
                    SetPrimarySelectionRange(start, end);
                    primarySet = true;
                }
                else
                {
                    AddSelectionRange(start, end);
                }
            }
        }

        public void SetPrimaryActivePosition(Cursor position)
        {
            ClearAllSelections();
            PrimarySelectionRange.ShiftHeadToPosition(position.Line, position.ColumnNumber);
        }

        public void SetActivePosition(int caretIndex, Cursor position)
        {
            _selectionRanges[caretIndex].ShiftHeadToPosition(position.Line, position.ColumnNumber);
        }

        public void InvertCaretOrder()
        {
            _selectionRanges.Reverse();
        }

        public void AddSelectionRange(Cursor? start, Cursor end)
        {
            SelectionRange selectionRange = new SelectionRange(start, end);
            _selectionRanges.Add(selectionRange);
        }

        public void SetPrimarySelectionRange(Cursor? start, Cursor end)
        {
            ClearAllSelections();
            PrimarySelectionRange.SelectRange(start, end);
        }

        public void SetSelectionRange(int caretIndex, Cursor? start, Cursor end)
        {
            if (caretIndex < _selectionRanges.Count)
            {
                _selectionRanges[caretIndex].SelectRange(start, end);
            }
            else if (caretIndex == _selectionRanges.Count)
            {
                AddSelectionRange(start, end);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(caretIndex));
            }
        }

        public void ClearAllSelections()
        {
            SelectionRange primary = PrimarySelectionRange;
            primary.CancelSelection();
            _selectionRanges.Clear();
            _selectionRanges.Add(primary);
        }

        public IEnumerator<SelectionRange> GetEnumerator()
        {
            return _selectionRanges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _selectionRanges.GetEnumerator();
        }

        internal IReadOnlyList<SourceCodePosition> GetPositions()
        {
            return _selectionRanges.Select(x => x.Head.GetPosition()).ToList();
        }
    }
}
