using CSharpTextEditor.Source;
using System;

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

        public override void Redo(SourceCode sourceCode)
        {
            if (ForwardInsertion)
            {
                Cursor head = sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber);
                RestoreIndent(head);
            }
            else
            {
                RemoveIndent(sourceCode);
            }
        }

        public override void Undo(SourceCode sourceCode)
        {
            if (ForwardInsertion)
            {
                RemoveIndent(sourceCode);
            }
            else
            {
                Cursor head = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
                RestoreIndent(head);
            }
        }

        private void RemoveIndent(SourceCode sourceCode)
        {
            Cursor tail = sourceCode.GetCursor(PositionBefore.LineNumber, PositionBefore.ColumnNumber);
            Cursor head = sourceCode.GetCursor(PositionAfter.LineNumber, PositionAfter.ColumnNumber);
            SelectionRange range = new SelectionRange(tail, head);
            range.RemoveSelectedRange(null);
        }

        private void RestoreIndent(Cursor head)
        {
            int diff = Math.Abs(PositionAfter.ColumnNumber - PositionBefore.ColumnNumber);
            foreach (char c in new string(' ', diff))
            {
                head.InsertCharacter(c);
            }
        }
    }
}