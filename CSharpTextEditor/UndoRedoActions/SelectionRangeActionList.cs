using CSharpTextEditor.Source;
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

        public SourceCodePosition? TailBefore { get; set; }

        public SourceCodePosition? TailAfter { get; set; }

        public SourceCodePosition? HeadBefore { get; set; }

        public SourceCodePosition? HeadAfter { get; set; }
    }
}
