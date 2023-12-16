using System.Collections.Generic;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class HistoryItem
    {
        public IReadOnlyCollection<UndoRedoAction> Actions { get; }

        public string DisplayName { get; }

        public HistoryItem(IReadOnlyCollection<UndoRedoAction> actions, string displayName)
        {
            Actions = actions;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }
}
