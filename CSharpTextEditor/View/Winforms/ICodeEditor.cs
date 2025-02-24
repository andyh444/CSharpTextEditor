﻿namespace CSharpTextEditor.View.Winforms
{
    public interface ICodeEditor
    {
        void SelectAll();
        void CutSelectedToClipboard();
        void CopySelectedToClipboard();
        void PasteFromClipboard();
        void Undo();
        void Redo();
        void ShiftActivePositionOneWordToTheLeft(bool select);
        void ShiftActivePositionOneWordToTheRight(bool select);
        void GoToFirstPosition();
        void GoToLastPosition();
        void RemoveWordBeforeActivePosition();
        void RemoveWordAfterActivePosition();
        void ScrollView(int numberOfLines);
        void DuplicateSelection();
        void SelectionToLowerCase();
        void SelectionToUpperCase();
    }
}
