using CSharpTextEditor.Source;
using System.Collections.Generic;
using System.Linq;

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

        public void MoveToBeforePositions(SourceCode sourceCode)
        {
            sourceCode.SelectRanges(Actions.Select(action => (action.TailBefore, action.HeadBefore)));
        }

        public void MoveToAfterPositions(SourceCode sourceCode)
        {
            sourceCode.SelectRanges(Actions.Select(action => (action.TailAfter, action.HeadAfter)));
        }
    }
}
