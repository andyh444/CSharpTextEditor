using System.Collections.Generic;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class SelectionRangeActionList
    {
        public SelectionRangeActionList(int index)
        {
            Index = index;
            UndoRedoActions = new List<UndoRedoAction>();
        }

        public int Index { get; }

        public List<UndoRedoAction> UndoRedoActions { get; }

        public CursorMoveAction? CursorMoveAction { get; set; }
    }
}
