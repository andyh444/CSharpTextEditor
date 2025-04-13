using NTextEditor.Source;
using System.Collections.Generic;
using System.Linq;

namespace NTextEditor.UndoRedoActions
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
            sourceCode.SelectRanges(GetAfterPositions());
        }

        private IEnumerable<(SourceCodePosition?, SourceCodePosition)> GetAfterPositions()
        {
            // not every action has a head after (e.g. if two ranges got merged into one during the action)
            foreach (var action in Actions)
            {
                if (action.HeadAfter != null)
                {
                    yield return (action.TailAfter, action.HeadAfter.Value);
                }
            }
        }
    }
}
