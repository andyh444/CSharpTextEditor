namespace CSharpTextEditor.UndoRedoActions
{
    internal class TabInsertionDeletionAction : UndoRedoAction
    {
        public bool ForwardInsertion { get; }

        public TabInsertionDeletionAction(bool forwardInsertion, SourceCodePosition positionBefore, SourceCodePosition positionAfter)
            : base(positionBefore, positionAfter)
        {
            ForwardInsertion = forwardInsertion;
        }

        public override void Redo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor head = sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber);
            if (ForwardInsertion)
            {
                head.IncreaseIndent();
            }
            else
            {
                head.DecreaseIndent();
            }
        }

        public override void Undo(SourceCode sourceCode, bool multipleCursors)
        {
            Cursor head = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
            if (ForwardInsertion)
            {
                head.DecreaseIndent();
            }
            else
            {
                head.IncreaseIndent();
            }
        }
    }
}