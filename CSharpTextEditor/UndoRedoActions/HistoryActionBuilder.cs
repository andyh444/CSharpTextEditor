using System.Collections.Generic;
using System.Linq;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class HistoryActionBuilder
    {
        private readonly List<SelectionRangeActionList> _actions;

        public HistoryActionBuilder()
        {
            _actions = new List<SelectionRangeActionList>();
        }

        public SelectionRangeActionList Add(int index)
        {
            SelectionRangeActionList newList = new SelectionRangeActionList(index);
            _actions.Add(newList);
            return newList;
        }

        public bool Any() => _actions.Any(x => x.UndoRedoActions.Any());

        public HistoryItem Build(string displayName)
        {
            return new HistoryItem(_actions, displayName);
        }
    }
}
