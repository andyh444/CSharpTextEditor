using System;
using System.Collections;
using System.Collections.Generic;
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
            _selectionRanges.ForEach(action);
        }

        public void SetPrimaryActivePosition(Cursor position)
        {
            ClearAllSelections();
            PrimarySelectionRange.UpdateHead(position.Line, position.LineNumber, position.ColumnNumber);
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
    }
}
