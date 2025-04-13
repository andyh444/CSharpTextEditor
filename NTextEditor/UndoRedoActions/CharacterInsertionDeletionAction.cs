using NTextEditor.Source;

namespace NTextEditor.UndoRedoActions
{
    internal class CharacterInsertionDeletionAction : UndoRedoAction
    {
        public char Character { get; }

        public bool ForwardInsertion { get; }

        public bool BeforeCursor { get; }

        public CharacterInsertionDeletionAction(char character, bool forwardInsertion, bool beforeCursor, SourceCodePosition positionBefore, SourceCodePosition positionAfter)
            :base(positionBefore, positionAfter)
        {
            Character = character;
            ForwardInsertion = forwardInsertion;
            BeforeCursor = beforeCursor;
        }

        public override void Redo(SourceCode sourceCode)
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

        public override void Undo(SourceCode sourceCode)
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
            if (!BeforeCursor)
            {
                head.ColumnNumber++;
            }
            head.Line.Value.RemoveCharacterBefore(head.ColumnNumber);
        }

        private void InsertCharacter(SourceCode sourceCode, SourceCodePosition position)
        {
            var head = sourceCode.GetCursor(position.LineNumber, position.ColumnNumber);
            head.InsertCharacter(Character);
            if (!BeforeCursor)
            {
                head.ColumnNumber--;
            }
        }
    }
}
