namespace CSharpTextEditor.UndoRedoActions
{
    internal class LineBreakInsertionDeletionAction : UndoRedoAction
    {
        public bool ForwardInsertion { get; }

        public LineBreakInsertionDeletionAction(bool forwardInsertion, SourceCodePosition positionBefore, SourceCodePosition positionAfter)
            : base(positionBefore, positionAfter)
        {
            ForwardInsertion = forwardInsertion;
        }

        public override void Redo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor cursor = sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber);
            if (ForwardInsertion)
            {
                cursor.InsertLineBreak();
            }
            else
            {
                SelectionRange range = new SelectionRange(null, cursor);
                while (cursor.ColumnNumber > 0)
                {
                    range.RemoveCharacterBeforeHead(null);
                }
                range.RemoveCharacterBeforeHead(null);
            }
        }

        public override void Undo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor cursor = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
            if (ForwardInsertion)
            {
                SelectionRange range = new SelectionRange(null, cursor);
                while (cursor.ColumnNumber > 0)
                {
                    range.RemoveCharacterBeforeHead(null);
                }
                range.RemoveCharacterBeforeHead(null);
            }
            else
            {
                cursor.InsertLineBreak();
            }
        }
    }
}
