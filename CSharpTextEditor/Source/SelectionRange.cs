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
        public EditResult RemoveSelectedRange(List<UndoRedoAction>? actionBuilder)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(false);
            while (last > first)
            {
                RemoveCharacterBeforePosition(last, actionBuilder);
            }
            CancelSelection();
            return new EditResult(0);
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

        public EditResult RemoveCharacterBeforeActivePosition(List<UndoRedoAction>? actionBuilder)
        {
            if (IsRangeSelected())
            {
                return RemoveSelectedRange(actionBuilder);
            }
            else
            {
                return RemoveCharacterBeforePosition(Head, actionBuilder);
            }
        }

        private EditResult RemoveCharacterBeforePosition(Cursor head, List<UndoRedoAction>? actionBuilder)
        {
            var before = head.GetPosition();
            char character = head.Line.Value.GetCharacterAtIndex(head.ColumnNumber - 1);
            if (head.Line.Value.RemoveCharacterBefore(head.ColumnNumber))
            {
                head.ColumnNumber--;
                actionBuilder?.Add(new CharacterInsertionDeletionAction(character, false, true, before, head.GetPosition()));
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
            return new EditResult(0);
        }

        public EditResult RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction>? actionBuilder)
        {
            if (IsRangeSelected())
            {
                (Cursor start, _) = GetOrderedCursors(false);
                start.ShiftOneWordToTheLeft(syntaxHighlighter);
                RemoveSelectedRange(actionBuilder);
            }
            else
            {
                Cursor startOfPreviousWord = Head.Clone();
                startOfPreviousWord.ShiftOneWordToTheLeft(syntaxHighlighter);
                RemoveRange(startOfPreviousWord, Head, actionBuilder);
            }
            return new EditResult(0);
        }

        public EditResult RemoveCharacterAfterActivePosition(List<UndoRedoAction>? actionBuilder)
        {
            EditResult? result = null;
            char character = Head.Line.Value.GetCharacterAtIndex(Head.ColumnNumber);
            if (IsRangeSelected())
            {
                result = RemoveSelectedRange(actionBuilder);
            }
            else
            {
                if (Head.Line.Value.RemoveCharacterAfter(Head.ColumnNumber))
                {
                    result = new EditResult(-1);
                    actionBuilder?.Add(new CharacterInsertionDeletionAction(character, false, false, Head.GetPosition(), Head.GetPosition()));
                }
                else if (Head.Line.Next != null
                    && Head.Line.List != null)
                {
                    Head.Line.Value.AppendText(Head.Line.Next.Value.Text);
                    Head.Line.List.Remove(Head.Line.Next);
                    result = new EditResult(-1);
                }
            }
            Head.ResetMaxColumnNumber();
            return result ?? new EditResult(0);
        }

        public EditResult RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction>? actionBuilder)
        {
            if (IsRangeSelected())
            {
                (_, Cursor end) = GetOrderedCursors(false);
                Cursor before = end.Clone();
                end.ShiftOneWordToTheRight(syntaxHighlighter);
                RemoveSelectedRange(actionBuilder);
                return new EditResult(end.ColumnNumber - before.ColumnNumber);
            }
            else
            {
                Cursor startOfNextWord = Head.Clone();
                Cursor before = startOfNextWord.Clone();
                startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
                int diff = before.ColumnNumber - startOfNextWord.ColumnNumber;
                RemoveRange(Head, startOfNextWord, actionBuilder);
                return new EditResult(diff);
            }
        }
        #endregion

        #region Insertion/Editing actions
        public EditResult InsertLineBreakAtActivePosition(SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler = null)
        {
            if (Tail != null)
            {
                RemoveSelectedRange(actionBuilder);
            }
            SourceCodePosition positionBefore = Head.GetPosition();
            Head.InsertLineBreak();
            actionBuilder?.Add(new LineBreakInsertionDeletionAction(true, positionBefore, Head.GetPosition()));
            specialCharacterHandler?.HandleLineBreakInserted(sourceCode, this, actionBuilder);
            return new EditResult(0);
        }

        public EditResult InsertCharacterAtActivePosition(char character, SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler, bool overtypeEnabled)
        {
            if (Tail != null)
            {
                RemoveSelectedRange(actionBuilder);
                overtypeEnabled = false;
            }
            var headBefore = Head.GetPosition();
            int positionChangeAfter = 0;
            if (character == '\t')
            {
                IncreaseIndentAtActivePosition(actionBuilder, new HashSet<int>());
            }
            else
            {
                
                specialCharacterHandler?.HandleCharacterInserting(character, sourceCode);
                Head.InsertCharacter(character);
                actionBuilder?.Add(new CharacterInsertionDeletionAction(character, true, true, headBefore, Head.GetPosition()));
                if (overtypeEnabled)
                {
                    char charToRemove = Head.Line.Value.GetCharacterAtIndex(Head.ColumnNumber);
                    if (Head.Line.Value.RemoveCharacterAfter(Head.ColumnNumber))
                    {
                        actionBuilder?.Add(new CharacterInsertionDeletionAction(charToRemove, false, false, Head.GetPosition(), Head.GetPosition()));
                        positionChangeAfter = -1;
            }
            return new EditResult(0);
        }
            }
            return new EditResult(positionChangeAfter);
        }

        public EditResult InsertStringAtActivePosition(string text, SourceCode sourceCode, List<UndoRedoAction>? actionBuilder, ISpecialCharacterHandler? specialCharacterHandler)
        {
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
                    InsertLineBreakAtActivePosition(sourceCode, actionBuilder, specialCharacterHandler);
                }
                firstLine = false;
                foreach (char ch in currentLine)
                {
                    var before = Head.GetPosition();
                    Head.InsertCharacter(ch);
                    actionBuilder?.Add(new CharacterInsertionDeletionAction(ch, true, true, before, Head.GetPosition()));
                }
            }

            Head.ResetMaxColumnNumber();
            return new EditResult(0);
        }

        internal EditResult DuplicateSelection(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (!IsRangeSelected())
            {
                // in this case, duplicate the entire line that the head is on
                Tail = null;
                SourceCodePosition positionBefore = Head.GetPosition();
                string lineText = Head.Line.Value.Text;
                Head.ShiftToEndOfLine();
                InsertLineBreakAtActivePosition(sourceCode, actionBuilder, null);
                InsertStringAtActivePosition(lineText, sourceCode, actionBuilder, null);
                Head.ColumnNumber = positionBefore.ColumnNumber;
                Tail?.CopyFrom(Head);
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
            return new EditResult(0);
        }

        internal EditResult SelectionToLowerCase(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                string text = GetSelectedText().ToLower();
                RemoveSelectedRange(actionBuilder);
                Cursor newTail = Head.Clone();
                InsertStringAtActivePosition(text, sourceCode, actionBuilder, null);
                SelectRange(newTail, Head.Clone());
            }
            return new EditResult(0);
        }

        internal EditResult SelectionToUpperCase(SourceCode sourceCode, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                string text = GetSelectedText().ToUpper();
                RemoveSelectedRange(actionBuilder);
                Cursor newTail = Head.Clone();
                InsertStringAtActivePosition(text, sourceCode, actionBuilder, null);
                SelectRange(newTail, Head.Clone());
            }
            return new EditResult(0);
        }
        #endregion

        #region Indenting actions
        private EditResult IncreaseIndentOnSelectedLines(List<UndoRedoAction>? actionBuilder, HashSet<int> linesToIgnore)
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    if (!linesToIgnore.Contains(start.LineNumber))
                    {
                        var indentBefore = new SourceCodePosition(start.LineNumber, 0);
                        start.Line.Value.IncreaseIndentAtPosition(0, out int shift);
                        actionBuilder?.Add(new TabInsertionDeletionAction(true, indentBefore, new SourceCodePosition(start.LineNumber, shift)));
                        linesToIgnore.Add(start.LineNumber);
                    }
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail!.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
                Head.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
            }
            return new EditResult(0);
        }

        private EditResult DecreaseIndentOnSelectedLines(List<UndoRedoAction>? actionBuilder, HashSet<int> linesToIgnore)
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    if (!linesToIgnore.Contains(start.LineNumber))
                    {
                        int firstNonWhiteSpaceIndex = start.Line.Value.FirstNonWhiteSpaceIndex;
                        var indentBefore = new SourceCodePosition(start.LineNumber, firstNonWhiteSpaceIndex);
                        start.Line.Value.DecreaseIndentAtPosition(firstNonWhiteSpaceIndex, out int shift);
                        if (shift > 0)
                        {
                            actionBuilder?.Add(new TabInsertionDeletionAction(false, indentBefore, new SourceCodePosition(start.LineNumber, firstNonWhiteSpaceIndex - shift)));
                            linesToIgnore.Add(start.LineNumber);
                        }
                    }
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail!.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
                Head.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
            }
            return new EditResult(0);
        }

        public EditResult IncreaseIndentAtActivePosition(List<UndoRedoAction>? actionBuilder, HashSet<int> linesToIgnore)
        {
            if (SelectionCoversMultipleLines())
            {
                return IncreaseIndentOnSelectedLines(actionBuilder, linesToIgnore);
            }
            if (IsRangeSelected())
            {
                RemoveSelectedRange(actionBuilder);
            }
            bool ignore = Head.ColumnNumber == Head.Line.Value.FirstNonWhiteSpaceIndex
                        && linesToIgnore.Contains(Head.LineNumber);
            if (!ignore)
            {
                SourceCodePosition headBefore = Head.GetPosition();
                Head.IncreaseIndent();
                actionBuilder?.Add(new TabInsertionDeletionAction(true, headBefore, Head.GetPosition()));
                linesToIgnore.Add(Head.LineNumber);
            }
            return new EditResult(0);
        }

        public EditResult DecreaseIndentAtActivePosition(List<UndoRedoAction>? actionBuilder, HashSet<int> linesToIgnore)
        {
            if (IsRangeSelected())
            {
                if (SelectionCoversMultipleLines())
                {
                    DecreaseIndentOnSelectedLines(actionBuilder, linesToIgnore);
                }
                else
                {
                    (Cursor first, Cursor last) = GetOrderedCursors(true);
                    bool ignore = first.ColumnNumber == first.Line.Value.FirstNonWhiteSpaceIndex
                        && linesToIgnore.Contains(first.LineNumber);
                    if (!ignore)
                    {
                        SourceCodePosition firstBefore = first.GetPosition();
                        first.DecreaseIndent();
                        SourceCodePosition firstAfter = first.GetPosition();
                        int diff = firstAfter.ColumnNumber - firstBefore.ColumnNumber;
                        Head.ShiftPosition(diff);
                        Tail!.ShiftPosition(diff);
                        if (!firstBefore.Equals(firstAfter))
                        {
                            actionBuilder?.Add(new TabInsertionDeletionAction(false, firstBefore, firstAfter));
                            linesToIgnore.Add(first.LineNumber);
                        }
                    }
                }
            }
            else
            {
                bool ignore = Head.ColumnNumber == Head.Line.Value.FirstNonWhiteSpaceIndex
                        && linesToIgnore.Contains(Head.LineNumber);
                if (!ignore)
                {
                    SourceCodePosition headBefore = Head.GetPosition();
                    Head.DecreaseIndent();
                    SourceCodePosition headAfter = Head.GetPosition();
                    if (!headBefore.Equals(headAfter))
                    {
                        actionBuilder?.Add(new TabInsertionDeletionAction(false, headBefore, headAfter));
                        linesToIgnore.Add(Head.LineNumber);
                    }
                }
            }
            return new EditResult(0);
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

        public bool OverlapsWith(SelectionRange other)
        {
            if (!IsRangeSelected()
                && !other.IsRangeSelected())
            {
                return Head.GetPosition().Equals(other.Head.GetPosition());
            }
            if (IsRangeSelected()
                && !other.IsRangeSelected())
            {
                return Contains(other.Head.GetPosition());
            }
            if (!IsRangeSelected()
                && other.IsRangeSelected())
            {
                return other.Contains(Head.GetPosition());
            }
            return Contains(other.Head.GetPosition())
                || Contains(other.Tail!.GetPosition())
                || other.Contains(Head.GetPosition())
                || other.Contains(Tail!.GetPosition());
        }

        public void Merge(SelectionRange other)
        {
            if (!IsRangeSelected()
                && !other.IsRangeSelected())
            {
                return;
            }

            bool tailFirst = Tail! < Head;

            // this: []
            // other: <>

            // case 1: [ <> ]
            // this fully contains other
            if (Contains(other.Head.GetPosition())
                && Contains(other.Tail!.GetPosition()))
            {
                // nothing to be done
            }

            // case 2: < [] >
            // other fully contains this
            else if (other.Contains(Head.GetPosition())
                && other.Contains(Tail!.GetPosition()))
            {
                Head.CopyFrom(other.Head);
                Tail = other.Tail!.Clone();

            }

            // case 3: [ < ] > or < [ > ]
            // partial overlap
            else
            {
                (Cursor thisFirst, Cursor thisLast) = GetOrderedCursors();
                (Cursor otherFirst, Cursor otherLast) = other.GetOrderedCursors();

                Cursor newFirst = thisFirst < otherFirst ? thisFirst : otherFirst;
                Cursor newLast = thisLast > otherLast ? thisLast : otherLast;

                Tail!.CopyFrom(newFirst);
                Head.CopyFrom(newLast);
            }

            bool newTailFirst = Tail! < Head;
            if (tailFirst != newTailFirst)
            {
                SwapTailAndHead();
            }
        }

        private bool Contains(SourceCodePosition position)
        {
            if (!IsRangeSelected())
            {
                return false;
            }
            (Cursor first, Cursor last) = GetOrderedCursors();
            return Maths.IsBetweenInclusive(first.GetPosition(), position, last.GetPosition());
        }

        private void SwapTailAndHead()
        {
            if (!IsRangeSelected())
            {
                return;
            }
            Cursor? temp = Tail;
            Tail = Head.Clone();
            Head.CopyFrom(temp!);
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
            // TODO: Move this
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
