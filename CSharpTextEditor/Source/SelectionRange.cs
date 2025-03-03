using CSharpTextEditor.Languages;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Source
{
    internal class SelectionRange
    {
        /// <summary>
        /// The other end of the selection. If there is no text selected, then this will be null
        /// </summary>
        public Cursor? Tail { get; private set; }

        /// <summary>
        /// The "active" position of the cursor. Characters will be removed/added, etc from this position
        /// </summary>
        public Cursor Head { get; }

        public SelectionRange(ISourceCodeLineNode initialLine, int initialColumnNumber)
            : this(null, new Cursor(initialLine, initialColumnNumber))
        {
        }

        public SelectionRange(Cursor? tail, Cursor head)
        {
            Tail = tail;
            Head = head;
        }

        #region Remove actions
        public void RemoveSelectedRange(List<UndoRedoAction>? actionBuilder)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(false);
            while (last > first)
            {
                RemoveCharacterBeforePosition(last, actionBuilder);
            }
            CancelSelection();
        }

        private void RemoveRange(Cursor start, Cursor end, List<UndoRedoAction>? actionBuilder)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(start, end);
            while (last > first)
            {
                RemoveCharacterBeforePosition(last, actionBuilder);
            }
            CancelSelection();
        }

        public void RemoveCharacterBeforeActivePosition(List<UndoRedoAction>? actionBuilder)
        {
            var headBefore = Head.GetPosition();
            var tailBefore = Tail?.GetPosition();
            if (IsRangeSelected())
            {
                RemoveSelectedRange(actionBuilder);
            }
            else
            {
                RemoveCharacterBeforePosition(Head, actionBuilder);
            }
            //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
        }

        private void RemoveCharacterBeforePosition(Cursor head, List<UndoRedoAction>? actionBuilder)
        {
            var before = head.GetPosition();
            char character = head.Line.Value.GetCharacterAtIndex(head.ColumnNumber - 1);
            if (head.Line.Value.RemoveCharacterBefore(head.ColumnNumber))
            {
                head.ColumnNumber--;
                actionBuilder?.Add(new CharacterInsertionDeletionAction(character, false, before, head.GetPosition()));
            }
            else if (head.Line.Previous != null
                && head.Line.List != null)
            {
                var list = head.Line.List;
                var oldCurrent = head.Line;

                int columnNumber = head.Line.Previous.Value.Text.Length;
                int lineNumber = head.LineNumber - 1;
                head.Line = head.Line.Previous;
                head.ColumnNumber = columnNumber;
                head.Line.Value.Text += oldCurrent.Value.Text;
                list.Remove(oldCurrent);
                actionBuilder?.Add(new LineBreakInsertionDeletionAction(false, before, head.GetPosition()));
            }
            head.ResetMaxColumnNumber();
        }

        public void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction>? actionBuilder)
        {
            SourceCodePosition headBefore = Head.GetPosition();
            SourceCodePosition? tailBefore = Tail?.GetPosition();
            if (IsRangeSelected())
            {
                RemoveSelectedRange(actionBuilder);
            }
            // TODO: This is problematic as syntaxHighlighter will be out of date after RemoveSelectedRange is called
            Cursor startOfPreviousWord = Head.Clone();
            startOfPreviousWord.ShiftOneWordToTheLeft(syntaxHighlighter);
            RemoveRange(startOfPreviousWord, Head, actionBuilder);
            //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), startOfPreviousWord.GetPosition()));
        }

        public void RemoveCharacterAfterActivePosition(List<UndoRedoAction>? actionBuilder)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange(actionBuilder);
            }
            else
            {
                if (Head.Line.Value.RemoveCharacterAfter(Head.ColumnNumber))
                {

                }
                else if (Head.Line.Next != null
                    && Head.Line.List != null)
                {
                    Head.Line.Value.AppendText(Head.Line.Next.Value.Text);
                    Head.Line.List.Remove(Head.Line.Next);
                }
            }
            Head.ResetMaxColumnNumber();
        }

        public void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction>? actionBuilder)
        {
            var tailBefore = Tail?.GetPosition();
            if (IsRangeSelected())
            {
                RemoveSelectedRange(actionBuilder);
            }
            // TODO: This is problematic as syntaxHighlighter will be out of date after RemoveSelectedRange is called
            Cursor startOfNextWord = Head.Clone();
            startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
            RemoveRange(Head, startOfNextWord, actionBuilder);
            //actionBuilder?.Add(new CursorMoveAction(tailBefore, startOfNextWord.GetPosition(), Tail?.GetPosition(), startOfNextWord.GetPosition()));
        }

        public void RemoveTabFromBeforeActivePosition(List<UndoRedoAction>? actionBuilder) => RemoveTabFromBeforePosition(Head, actionBuilder);

        private void RemoveTabFromBeforePosition(Cursor position, List<UndoRedoAction>? actionBuilder)
        {
            // TODO: Different behaviour if there is a range selected
            if (position.AtStartOfLine())
            {
                return;
            }
            string textBeforePosition = position.GetLineValue().Substring(0, position.ColumnNumber);
            if (string.IsNullOrWhiteSpace(textBeforePosition))
            {
                Cursor otherEnd = position.Clone();
                otherEnd.ColumnNumber = Math.Max(0, otherEnd.ColumnNumber - SourceCode.TAB_REPLACEMENT.Length);
                RemoveRange(otherEnd, position, actionBuilder);
            }
        }
        #endregion

        #region Insertion/Editing actions
        public void InsertLineBreakAtActivePosition(SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler = null, bool addMoveAction = true)
        {
            SourceCodePosition? tailBefore = Tail?.GetPosition();
            SourceCodePosition headBefore = Head.GetPosition();
            if (Tail != null)
            {
                RemoveSelectedRange(actionBuilder);
            }
            Head.InsertLineBreak();
            actionBuilder?.Add(new LineBreakInsertionDeletionAction(true, headBefore, Head.GetPosition()));
            specialCharacterHandler?.HandleLineBreakInserted(sourceCode, this, actionBuilder);
            if (addMoveAction)
            {
                //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
            }
        }

        public void InsertCharacterAtActivePosition(char character, SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler)
        {
            var tailBefore = Tail?.GetPosition();
            var headBefore = Head.GetPosition();
            if (Tail != null)
            {
                RemoveSelectedRange(actionBuilder);
            }
            if (character == '\t')
            {
                Head.Line.Value.IncreaseIndentAtPosition(Head.ColumnNumber, out _);
                return;
            }
            specialCharacterHandler?.HandleCharacterInserting(character, sourceCode);
            Head.InsertCharacter(character);
            actionBuilder?.Add(new CharacterInsertionDeletionAction(character, true, headBefore, Head.GetPosition()));
            //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
        }

        public void InsertStringAtActivePosition(string text, SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler, bool insertMoveAction = true)
        {
            var headBefore = Head.GetPosition();
            var tailBefore = Tail?.GetPosition();
            if (Tail != null)
            {
                RemoveSelectedRange(actionBuilder);
            }
            text = text.Replace("\t", SourceCode.TAB_REPLACEMENT);
            bool firstLine = true;
            foreach (string currentLine in text.SplitIntoLines())
            {
                if (!firstLine)
                {
                    InsertLineBreakAtActivePosition(sourceCode, actionBuilder, specialCharacterHandler, false);
                }
                firstLine = false;
                foreach (char ch in currentLine)
                {
                    var before = Head.GetPosition();
                    Head.InsertCharacter(ch);
                    actionBuilder?.Add(new CharacterInsertionDeletionAction(ch, true, before, Head.GetPosition()));
                }
            }

            Head.ResetMaxColumnNumber();
            if (insertMoveAction)
            {
                //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
            }
        }

        internal void DuplicateSelection(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (!IsRangeSelected())
            {
                // in this case, duplicate the entire line that the head is on
                Tail = null;
                SourceCodePosition positionBefore = Head.GetPosition();
                string lineText = Head.Line.Value.Text;
                Head.ShiftToEndOfLine();
                InsertLineBreakAtActivePosition(sourceCode, actionBuilder, null, false);
                InsertStringAtActivePosition(lineText, sourceCode, actionBuilder, null, false);
                Head.ColumnNumber = positionBefore.ColumnNumber;
                SourceCodePosition positionAfter = Head.GetPosition();
                Tail?.CopyFrom(Head);
                //actionBuilder?.Add(new CursorMoveAction(null, positionBefore, null, positionAfter));
            }
            else
            {
                // just duplicate the selection
                string selectedText = GetSelectedText();
                (Cursor start, Cursor end) = GetOrderedCursors();
                CancelSelection();
                Head.CopyFrom(end);
                InsertStringAtActivePosition(selectedText, sourceCode, actionBuilder, null);
                int advanceAmount = GetPositionCount(selectedText);
                for (int i = 0; i < advanceAmount; i++)
                {
                    start.ShiftOneCharacterToTheRight();
                    end.ShiftOneCharacterToTheRight();
                }
                SelectRange(start, end);
            }
        }

        internal void SelectionToLowerCase(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                string text = GetSelectedText().ToLower();
                RemoveSelectedRange(actionBuilder);
                Cursor newTail = Head.Clone();
                InsertStringAtActivePosition(text, sourceCode, actionBuilder, null);
                SelectRange(newTail, Head.Clone());
            }
        }

        internal void SelectionToUpperCase(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                string text = GetSelectedText().ToUpper();
                RemoveSelectedRange(actionBuilder);
                Cursor newTail = Head.Clone();
                InsertStringAtActivePosition(text, sourceCode, actionBuilder, null);
                SelectRange(newTail, Head.Clone());
            }
        }
        #endregion

        #region Indenting actions
        public void IncreaseIndentOnSelectedLines(List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                SourceCodePosition headBefore = Head.GetPosition();
                SourceCodePosition? tailBefore = Tail?.GetPosition();
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    var indentBefore = new SourceCodePosition(start.LineNumber, 0);
                    start.Line.Value.IncreaseIndentAtPosition(0, out int shift);
                    actionBuilder.Add(new TabInsertionDeletionAction(true, indentBefore, new SourceCodePosition(start.LineNumber, shift)));
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail!.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
                Head.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;

                //actionBuilder.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
            }
        }

        public void DecreaseIndentOnSelectedLines(List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                SourceCodePosition headBefore = Head.GetPosition();
                SourceCodePosition? tailBefore = Tail?.GetPosition();
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    int firstNonWhiteSpaceIndex = start.Line.Value.FirstNonWhiteSpaceIndex;
                    var indentBefore = new SourceCodePosition(start.LineNumber, firstNonWhiteSpaceIndex);
                    start.Line.Value.DecreaseIndentAtPosition(firstNonWhiteSpaceIndex, out int shift);
                    if (shift > 0)
                    {
                        actionBuilder.Add(new TabInsertionDeletionAction(false, indentBefore, new SourceCodePosition(start.LineNumber, firstNonWhiteSpaceIndex - shift)));
                    }
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail!.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
                Head.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
                //actionBuilder.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
            }
        }

        public void IncreaseIndentAtActivePosition(List<UndoRedoAction> actionBuilder)
        {
            SourceCodePosition headBefore = Head.GetPosition();
            SourceCodePosition? tailBefore = Tail?.GetPosition();
            Head.IncreaseIndent();
            actionBuilder.Add(new TabInsertionDeletionAction(true, headBefore, Head.GetPosition()));
            //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), Head.GetPosition()));
        }

        public void DecreaseIndentAtActivePosition(List<UndoRedoAction> actionBuilder)
        {
            SourceCodePosition headBefore = Head.GetPosition();
            SourceCodePosition? tailBefore = Tail?.GetPosition();
            Head.DecreaseIndent();
            SourceCodePosition headAfter = Head.GetPosition();
            if (!headBefore.Equals(headAfter))
            {
                actionBuilder.Add(new TabInsertionDeletionAction(false, headBefore, headAfter));
                //actionBuilder?.Add(new CursorMoveAction(tailBefore, headBefore, Tail?.GetPosition(), headAfter));
            }
        }
        #endregion

        public bool IsRangeSelected()
        {
            return Tail != null
                && !Tail.SamePositionAsOther(Head);
        }

        public bool SelectionCoversMultipleLines()
        {
            return Tail != null
                && Tail.LineNumber != Head.LineNumber;
        }

        public string GetSelectedText()
        {
            if (Tail == null)
            {
                return string.Empty;
            }
            (Cursor start, Cursor end) = GetOrderedCursors();
            // TODO: Do this line by line instead of character by character
            StringBuilder sb = new StringBuilder();
            while (start < end)
            {
                if (start.AtEndOfLine())
                {
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(start.Line.Value.GetCharacterAtIndex(start.ColumnNumber));
                }
                if (!start.ShiftOneCharacterToTheRight())
                {
                    break;
                }
            }
            return sb.ToString();
        }

        public (Cursor first, Cursor last) GetOrderedCursors(bool clone = true)
        {
            if (clone)
            {
                return GetOrderedCursors((Tail ?? Head).Clone(), Head.Clone());
            }
            return GetOrderedCursors(Tail ?? Head, Head);
        }

        public (Cursor first, Cursor last) GetOrderedCursors(Cursor tail, Cursor head)
        {
            if (tail > head)
            {
                return (head, tail);
            }
            return (tail, head);
        }

        #region Cursor movement
        public void CancelSelection()
        {
            Tail = null;
        }

        public void SelectRange(Cursor? start, Cursor end)
        {
            Tail = start;
            Head.CopyFrom(end);
        }

        public void ShiftHeadToPosition(Cursor other) => ShiftHeadToPosition(other.Line, other.ColumnNumber);

        public void ShiftHeadToPosition(ISourceCodeLineNode newLine, int newColumnIndex)
        {
            Head.Line = newLine;
            Head.ColumnNumber = newColumnIndex;
        }

        private void ShiftTailToPosition(bool selection)
        {
            if (!selection)
            {
                CancelSelection();
            }
            else if (Tail == null)
            {
                Tail = Head.Clone();
            }
        }

        public void ShiftHeadUpOneLine(bool selection)
        {
            ShiftTailToPosition(selection);
            Head.ShiftUpOneLine();
        }

        public void ShiftHeadDownOneLine(bool selection)
        {
            ShiftTailToPosition(selection);
            Head.ShiftDownOneLine();
        }

        public void ShiftHeadUpLines(int lineCount, bool selection)
        {
            ShiftTailToPosition(selection);
            Head.ShiftUpLines(lineCount);
        }

        public void ShiftHeadDownLines(int lineCount, bool selection)
        {
            ShiftTailToPosition(selection);
            Head.ShiftDownLines(lineCount);
        }

        public void ShiftHeadToTheLeft(bool selection)
        {
            if (!selection
                && IsRangeSelected())
            {
                if (Head > Tail!)
                {
                    ShiftHeadToPosition(Tail!);
                }
                CancelSelection();
                return;
            }
            ShiftTailToPosition(selection);
            Head.ShiftOneCharacterToTheLeft();
        }

        public void ShiftHeadToTheRight(bool selection = false)
        {
            if (!selection
                && IsRangeSelected())
            {
                if (Head < Tail!)
                {
                    ShiftHeadToPosition(Tail!);
                }
                CancelSelection();
                return;
            }
            ShiftTailToPosition(selection);
            Head.ShiftOneCharacterToTheRight();
        }

        public void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            ShiftTailToPosition(selection);
            Head.ShiftOneWordToTheRight(syntaxHighlighter);
        }

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            ShiftTailToPosition(selection);
            Head.ShiftOneWordToTheLeft(syntaxHighlighter);
        }

        public void ShiftHeadToEndOfLine(bool selection = false)
        {
            ShiftTailToPosition(selection);
            Head.ShiftToEndOfLine();
        }

        public void ShiftHeadToHome(bool selection = false)
        {
            ShiftTailToPosition(selection);
            Head.ShiftToHome();
        }
        #endregion

        private int GetPositionCount(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r')
                {
                    if (i < text.Length - 1
                        && text[i + 1] == '\n')
                    {
                        // new line only counts as one position
                        continue;
                    }
                }
                count++;
            }
            return count;
        }
    }
}
