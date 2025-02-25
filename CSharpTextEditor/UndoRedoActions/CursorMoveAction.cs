using CSharpTextEditor.Source;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class CursorMoveAction : UndoRedoAction
    {
        private readonly SourceCodePosition? _tailPositionBefore;
        private readonly SourceCodePosition? _tailPositionAfter;

        public CursorMoveAction(SourceCodePosition? tailBefore, SourceCodePosition headBefore, SourceCodePosition? tailAfter, SourceCodePosition headAfter)
            : base(headBefore, headAfter)
        {
            _tailPositionBefore = tailBefore;
            _tailPositionAfter = tailAfter;
        }

        public override void Redo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor headAfter = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
            Cursor? tailAfter = null;
            if (_tailPositionAfter != null)
            {
                tailAfter = sourceCode.GetCursor(_tailPositionAfter.Value.LineNumber, _tailPositionAfter.Value.ColumnNumber);
            }
            if (!multipleCursors)
            {
                sourceCode.SelectionRangeCollection.SetPrimarySelectionRange(tailAfter, headAfter);
            }
            else
            {
                sourceCode.SelectionRangeCollection.AddSelectionRange(tailAfter, headAfter);
            }
        }

        public override void Undo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor headBefore = sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber);
            Cursor? tailBefore = null;
            if (_tailPositionBefore != null)
            {
                tailBefore = sourceCode.GetCursor(_tailPositionBefore.Value.LineNumber, _tailPositionBefore.Value.ColumnNumber);
            }
            if (!multipleCursors)
            {
                sourceCode.SelectionRangeCollection.SetPrimarySelectionRange(tailBefore, headBefore);
            }
            else
            {
                sourceCode.SelectionRangeCollection.AddSelectionRange(tailBefore, headBefore);
            }
        }
    }
}
