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

        public void Add(SelectionRangeActionList actionList)
        {
            _actions.Add(actionList);
        }

        public bool Any() => _actions.Any(x => x.UndoRedoActions.Any());

        public HistoryItem Build(string displayName)
        {
            return new HistoryItem(_actions, displayName);
        }
    }
}
