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
            sourceCode.SelectionRangeCollection.SetSelectionRanges(Actions.Select(action =>
            {
                Cursor? tail = null;
                if (action.TailBefore.HasValue)
                {
                    tail = sourceCode.GetCursor(action.TailBefore.Value.LineNumber, action.TailBefore.Value.ColumnNumber);
                }
                Cursor head = sourceCode.GetCursor(action.HeadBefore!.Value.LineNumber, action.HeadBefore!.Value.ColumnNumber);

                return (tail, head);
            }));
        }

        public void MoveToAfterPositions(SourceCode sourceCode)
        {
            sourceCode.SelectionRangeCollection.SetSelectionRanges(Actions.Select(action =>
            {
                Cursor? tail = null;
                if (action.TailAfter.HasValue)
                {
                    tail = sourceCode.GetCursor(action.TailAfter.Value.LineNumber, action.TailAfter.Value.ColumnNumber);
                }
                Cursor head = sourceCode.GetCursor(action.HeadAfter!.Value.LineNumber, action.HeadAfter!.Value.ColumnNumber);

                return (tail, head);
            }));
        }
    }
}
