﻿using System.Windows.Forms;

namespace CSharpTextEditor.UndoRedoActions
{
    internal abstract class UndoRedoAction
    {
        public SourceCodePosition PositionBefore { get; }

        public SourceCodePosition PositionAfter { get; }

        protected UndoRedoAction(SourceCodePosition positionBefore, SourceCodePosition positionAfter)
        {
            PositionBefore = positionBefore;
            PositionAfter = positionAfter;
        }

        public abstract void Undo(SourceCode sourceCode, bool multipleCursors);

        public abstract void Redo(SourceCode sourceCode, bool multipleCursors);
    }
}
