using CSharpTextEditor.Source;

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

        public override void Redo(SourceCode sourceCode)
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
                    range.RemoveCharacterBeforeActivePosition(null);
                }
                range.RemoveCharacterBeforeActivePosition(null);
            }
        }

        public override void Undo(SourceCode sourceCode)
        {
            Cursor cursor = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
            if (ForwardInsertion)
            {
                SelectionRange range = new SelectionRange(null, cursor);
                while (cursor.ColumnNumber > 0)
                {
                    range.RemoveCharacterBeforeActivePosition(null);
                }
                range.RemoveCharacterBeforeActivePosition(null);
            }
            else
            {
                cursor.InsertLineBreak();
            }
        }
    }
}
