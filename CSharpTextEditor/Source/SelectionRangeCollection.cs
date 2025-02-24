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
            int index = _selectionRanges.Count - 1;
            foreach (var range in _selectionRanges.OrderByDescending(x => x.Head))
            {
                action(range, builder.Add(index).UndoRedoActions);
                index--;
            }
            if (builder.Any())
            {
                manager.AddAction(builder.Build(displayName));
            }
        }

        public void SetSelectionRanges(IEnumerable<(Cursor start, Cursor end)> ranges)
        {
            bool primarySet = false;
            foreach ((Cursor start, Cursor end) in ranges)
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
            _selectionRanges[caretIndex].SelectRange(start, end);
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
