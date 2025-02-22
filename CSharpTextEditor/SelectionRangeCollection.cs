using CSharpTextEditor.UndoRedoActions;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SelectionRangeCollection : IReadOnlyCollection<SelectionRange>
    {
        private readonly List<SelectionRange> _selectionRanges;

        public int Count => _selectionRanges.Count;

        public SelectionRange PrimarySelectionRange => _selectionRanges.First();

        public SelectionRangeCollection(LinkedListNode<SourceCodeLine> initialLine, int initialLineNumber, int initialColumnNumber)
        {
            _selectionRanges = new List<SelectionRange> { new SelectionRange(initialLine, initialLineNumber, initialColumnNumber) };
        }

        public void DoActionOnAllRanges(Action<SelectionRange> action)
        {
            _selectionRanges.ForEach(r => action(r));
        }

        public void DoActionOnAllRanges(Action<SelectionRange, List<UndoRedoAction>> action, HistoryManager manager, string displayName)
        {
            List<UndoRedoAction> actions = new List<UndoRedoAction>();
            _selectionRanges.ForEach(r => action(r, actions));
            if (actions.Count > 0)
            {
                manager.AddAction(new HistoryItem(actions, displayName));
            }
        }

        public void SetSelectionRanges(IEnumerable<(Cursor start, Cursor end)> ranges)
        {
            bool primarySet = false;
            foreach ((Cursor start, Cursor end) in ranges)
            {
                if (!primarySet)
                {
                    SetPrimaryRange(start, end);
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
            PrimarySelectionRange.UpdateHead(position.Line, position.LineNumber, position.ColumnNumber);
        }

        public void AddSelectionRange(Cursor? start, Cursor end)
        {
            SelectionRange selectionRange = new SelectionRange(start, end);
            _selectionRanges.Add(selectionRange);
        }

        public void SetPrimaryRange(Cursor start, Cursor end)
        {
            ClearAllSelections();
            PrimarySelectionRange.SelectRange(start, end);
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
