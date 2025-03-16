using CSharpTextEditor.Source;
using System.Collections.Generic;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class SelectionRangeActionList
    {
        public SelectionRangeActionList(List<UndoRedoAction> undoRedoActions, SourceCodePosition? tailBefore, SourceCodePosition? tailAfter, SourceCodePosition headBefore, SourceCodePosition? headAfter)
        {
            UndoRedoActions = undoRedoActions;
            TailBefore = tailBefore;
            TailAfter = tailAfter;
            HeadBefore = headBefore;
            HeadAfter = headAfter;
        }

        public List<UndoRedoAction> UndoRedoActions { get; }

        public SourceCodePosition? TailBefore { get; set; }

        public SourceCodePosition? TailAfter { get; set; }

        public SourceCodePosition HeadBefore { get; set; }

        public SourceCodePosition? HeadAfter { get; set; }
    }
}
