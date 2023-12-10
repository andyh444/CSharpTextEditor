namespace CSharpTextEditor.UndoRedoActions
{
    internal class CursorMoveAction : UndoRedoAction
    {
        public CursorMoveAction(SourceCodePosition positionBefore, SourceCodePosition positionAfter)
            : base(positionBefore, positionAfter)
        {
        }

        public override void Redo(SourceCode sourceCode, bool multipleCursors)
        {
            if (!multipleCursors)
            {
                sourceCode.SelectionRangeCollection.SetPrimaryActivePosition(sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber));
            }
            else
            {
                sourceCode.SelectionRangeCollection.AddSelectionRange(null, sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber));
            }
        }

        public override void Undo(SourceCode sourceCode, bool multipleCursors)
        {
            if (!multipleCursors)
            {
                sourceCode.SelectionRangeCollection.SetPrimaryActivePosition(sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber));
            }
            else
            {
                sourceCode.SelectionRangeCollection.AddSelectionRange(null, sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber));
            }
        }
    }
}
