using CSharpTextEditor.UndoRedoActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public void DoActionOnAllRanges(Action<SelectionRange> action, ISourceCodeListener? listener)
        {
            foreach (var range in _selectionRanges.OrderByDescending(x => x.Head))
            {
                action(range);
            }
            ResolveOverlappingRanges();
            listener?.CursorsChanged();
        }

        public void DoEditActionOnAllRanges(Func<SelectionRange, List<UndoRedoAction>, EditResult> action, HistoryManager manager, string displayName, ISourceCodeListener? listener)
        {
            HistoryActionBuilder builder = new HistoryActionBuilder();
            int index = 0;// _selectionRanges.Count - 1;

            List<SelectionRange> ordered = _selectionRanges.OrderBy(x => x.Head).ToList();
            List<SourceCodePosition?> originalTailPositions = ordered.Select(x => x.Tail?.GetPosition()).ToList();
            List<SourceCodePosition> originalHeadPositions = ordered.Select(x => x.Head.GetPosition()).ToList();

            
            foreach (var rangesOnSameLine in ordered.GroupBy(x => x.Head.LineNumber))
            {
                EditResult? previousEditResult = null;
                SelectionRange? previous = null;

                foreach (SelectionRange range in rangesOnSameLine)
                {
                    if (previousEditResult != null
                        && previous != null)
                    {
                        int headDifference = range.Head.ColumnNumber - originalHeadPositions[index - 1].ColumnNumber;
                        int? tailDifference = range.Tail?.GetPositionDifference(range.Head);
                        range.Head.CopyFrom(previous.Head);
                        range.Head.ShiftPosition(headDifference + previousEditResult.PositionChangeAfter);

                        range.Tail?.CopyFrom(range.Head);
                        range.Tail?.ShiftPosition(-(tailDifference) ?? 0);
                    }
                    List<UndoRedoAction> actions = new List<UndoRedoAction>();
                    previousEditResult = action(range, actions);
                    builder.Add(new SelectionRangeActionList(actions, originalTailPositions[index], range.Tail?.GetPosition(), originalHeadPositions[index], range.Head.GetPosition()));
                    index++;

                    previous = range;
                }
            }
            if (builder.Any())
            {
                manager.AddAction(builder.Build(displayName));
            }
            ResolveOverlappingRanges();
            listener?.TextChanged();
            listener?.CursorsChanged();
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

        public void ResolveOverlappingRanges()
        {
            if (_selectionRanges.Count < 2)
            {
                return;
            }
            List<SelectionRange> ranges = _selectionRanges.OrderBy(x => x.Head).ToList();
            bool done = false;
            while (!done)
            {
                done = true;
                for (int i = 0; i < ranges.Count - 1; i++)
                {
                    SelectionRange current = ranges[i];
                    SelectionRange next = ranges[i + 1];
                    if (current.OverlapsWith(next))
                    {
                        current.Merge(next);
                        ranges.RemoveAt(i + 1);
                        _selectionRanges.Remove(next);
                        done = false;
                    }
                }
            }
        }

        public IEnumerator<SelectionRange> GetEnumerator()
        {
            return _selectionRanges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _selectionRanges.GetEnumerator();
        }
    }
}
