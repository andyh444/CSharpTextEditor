using System.Collections.Generic;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class HistoryItem
    {
        public IReadOnlyCollection<SelectionRangeActionList> Actions { get; }

        public string DisplayName { get; }

        public HistoryItem(IReadOnlyCollection<SelectionRangeActionList> actions, string displayName)
        {
            Actions = actions;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }
}
