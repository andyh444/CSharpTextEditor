namespace CSharpTextEditor.UndoRedoActions
{
    internal class CharacterInsertionDeletionAction : UndoRedoAction
    {
        public char Character { get; }

        public bool ForwardInsertion { get; }

        public CharacterInsertionDeletionAction(char character, bool forwardInsertion, SourceCodePosition positionBefore, SourceCodePosition positionAfter)
            :base(positionBefore, positionAfter)
        {
            Character = character;
            ForwardInsertion = forwardInsertion;
        }

        public override void Redo(SourceCode sourceCode, bool multipleCursors)
        {
            if (ForwardInsertion)
            {
                InsertCharacter(sourceCode, PositionBefore);
            }
            else
            {
                DeleteCharacter(sourceCode, PositionBefore);
            }
        }

        public override void Undo(SourceCode sourceCode, bool multipleCursors)
        {
            if (ForwardInsertion)
            {
                DeleteCharacter(sourceCode, PositionAfter);
            }
            else
            {
                InsertCharacter(sourceCode, PositionAfter);
            }
        }

        private void DeleteCharacter(SourceCode sourceCode, SourceCodePosition position)
        {
            var head = sourceCode.GetCursor(position.LineNumber, position.ColumnNumber);
            head.Line.Value.RemoveCharacterBefore(head.ColumnNumber);
        }

        private void InsertCharacter(SourceCode sourceCode, SourceCodePosition position)
        {
            var head = sourceCode.GetCursor(position.LineNumber, position.ColumnNumber);
            head.InsertCharacter(Character);
        }
    }
}
