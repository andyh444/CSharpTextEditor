using CSharpTextEditor.UndoRedoActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SelectionRange
    {
        /// <summary>
        /// The other end of the selection. If there is no text selected, then this will be null
        /// </summary>
        public Cursor Tail { get; private set; }

        /// <summary>
        /// The "active" position of the cursor. Characters will be removed/added, etc from this position
        /// </summary>
        public Cursor Head { get; }

        public SelectionRange(LinkedListNode<SourceCodeLine> initialLine, int initialLineNumber, int initialColumnNumber)
            :this(null, new Cursor(initialLine, initialColumnNumber, initialLineNumber))
        {
        }

        public SelectionRange(Cursor tail, Cursor head)
        {
            Tail = tail;
            Head = head;
        }

        public void RemoveSelected(List<UndoRedoAction> actionBuilder)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(false);
            while (last > first)
            {
                RemoveCharacterBeforeHead(last, actionBuilder);
            }
            CancelSelection();
        }

        private void RemoveRange(Cursor start, Cursor end, List<UndoRedoAction> actionBuilder)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(start, end);
            while (last > first)
            {
                RemoveCharacterBeforeHead(last, actionBuilder);
            }
            CancelSelection();
        }

        public void RemoveCharacterBeforeHead(List<UndoRedoAction> actionBuilder)
        {
            var before = Head.GetPosition();
            if (IsRangeSelected())
            {
                RemoveSelected(actionBuilder);
            }
            else
            {
                RemoveCharacterBeforeHead(Head, actionBuilder);
            }
            actionBuilder?.Add(new CursorMoveAction(before, Head.GetPosition()));
        }

        private void RemoveCharacterBeforeHead(Cursor head, List<UndoRedoAction> actionBuilder)
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
                LinkedListNode<SourceCodeLine> oldCurrent = head.Line;

                int columnNumber = head.Line.Previous.Value.Text.Length;
                int lineNumber = head.LineNumber - 1;
                head.Line = head.Line.Previous;
                head.ColumnNumber = columnNumber;
                head.LineNumber = lineNumber;
                head.Line.Value.Text += oldCurrent.Value.Text;
                head.Line.List.Remove(oldCurrent);
                actionBuilder?.Add(new LineBreakInsertionDeletionAction(false, before, head.GetPosition()));
            }
            head.ResetMaxColumnNumber();
        }

        public void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                RemoveSelected(actionBuilder);
            }
            RemoveWordBeforePosition(Head, syntaxHighlighter, actionBuilder);
        }

        private void RemoveWordBeforePosition(Cursor position, ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction> actionBuilder)
        {
            Cursor startOfPreviousWord = position.Clone();
            startOfPreviousWord.ShiftOneWordToTheLeft(syntaxHighlighter);
            RemoveRange(startOfPreviousWord, position, actionBuilder);
        }

        public void RemoveCharacterAfterActivePosition(List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                RemoveSelected(actionBuilder);
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

        public void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction> actionBuilder)
        {
            if (IsRangeSelected())
            {
                RemoveSelected(actionBuilder);
            }
            RemoveWordAfterPosition(Head, syntaxHighlighter, actionBuilder);
        }

        private void RemoveWordAfterPosition(Cursor position, ISyntaxHighlighter syntaxHighlighter, List<UndoRedoAction> actionBuilder)
        {
            Cursor startOfNextWord = position.Clone();
            startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
            RemoveRange(position, startOfNextWord, actionBuilder);
        }

        public void InsertLineBreakAtActivePosition(SourceCode sourceCode, List<UndoRedoAction> actionBuilder, ISpecialCharacterHandler specialCharacterHandler = null, bool addMoveAction = true)
        {
            if (Tail != null)
            {
                RemoveSelected(actionBuilder);
            }
            SourceCodePosition before = Head.GetPosition();
            Head.InsertLineBreak();
            actionBuilder?.Add(new LineBreakInsertionDeletionAction(true, before, Head.GetPosition()));
            if (addMoveAction)
            {
                actionBuilder?.Add(new CursorMoveAction(before, Head.GetPosition()));
            }
            specialCharacterHandler?.HandleLineBreakInserted(sourceCode, Head);
        }

        public void InsertCharacterAtActivePosition(char character, SourceCode sourceCode, ISpecialCharacterHandler specialCharacterHandler, List<UndoRedoAction> actionBuilder)
        {
            if (Tail != null)
            {
                RemoveSelected(actionBuilder);
            }
            if (character == '\t')
            {
                Head.Line.Value.IncreaseIndentAtPosition(Head.ColumnNumber, out _);
                return;
            }
            specialCharacterHandler?.HandleCharacterInserting(character, sourceCode);
            var before = Head.GetPosition();
            Head.InsertCharacter(character);
            actionBuilder?.Add(new CharacterInsertionDeletionAction(character, true, before, Head.GetPosition()));
            actionBuilder?.Add(new CursorMoveAction(before, Head.GetPosition()));
        }

        public void InsertStringAtActivePosition(string text, SourceCode sourceCode, ISpecialCharacterHandler specialCharacterHandler, List<UndoRedoAction> actionBuilder)
        {
            if (Tail != null)
            {
                RemoveSelected(actionBuilder);
            }
            text = text.Replace("\t", SourceCode.TAB_REPLACEMENT);
            var start = Head.GetPosition();
            using (StringReader sr = new StringReader(text))
            {
                string currentLine = sr.ReadLine();
                while (currentLine != null)
                {
                    foreach (char ch in currentLine)
                    {
                        var before = Head.GetPosition();
                        Head.InsertCharacter(ch);
                        actionBuilder?.Add(new CharacterInsertionDeletionAction(ch, true, before, Head.GetPosition()));
                    }
                    currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        InsertLineBreakAtActivePosition(sourceCode, actionBuilder, specialCharacterHandler, false);
                    }
                }
            }
            Head.ResetMaxColumnNumber();
            actionBuilder?.Add(new CursorMoveAction(start, Head.GetPosition()));
        }


        public void CancelSelection()
        {
            Tail = null;
        }

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

        public void SelectRange(Cursor start, Cursor end)
        {
            Tail = start;
            Head.CopyFrom(end);
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
                start.ShiftOneCharacterToTheRight();
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

        public void UpdateHead(Cursor other) => UpdateHead(other.Line, other.LineNumber, other.ColumnNumber);

        public void UpdateHead(LinkedListNode<SourceCodeLine> newLine, int newLineIndex, int newColumnIndex)
        {
            Head.Line = newLine;
            Head.LineNumber = newLineIndex;
            Head.ColumnNumber = newColumnIndex;
        }

        private void UpdateTail(bool selection)
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
            UpdateTail(selection);
            Head.ShiftUpOneLine();
        }

        public void ShiftHeadDownOneLine(bool selection)
        {
            UpdateTail(selection);
            Head.ShiftDownOneLine();
        }

        public void ShiftHeadUpLines(int lineCount, bool selection)
        {
            UpdateTail(selection);
            Head.ShiftUpLines(lineCount);
        }

        public void ShiftHeadDownLines(int lineCount, bool selection)
        {
            UpdateTail(selection);
            Head.ShiftDownLines(lineCount);
        }

        public void ShiftHeadToTheLeft(bool selection)
        {
            if (!selection
                && IsRangeSelected())
            {
                if (Head > Tail)
                {
                    UpdateHead(Tail);
                }
                CancelSelection();
                return;
            }
            UpdateTail(selection);
            Head.ShiftOneCharacterToTheLeft();
        }

        public void ShiftHeadToTheRight(bool selection = false)
        {
            if (!selection
                && IsRangeSelected())
            {
                if (Head < Tail)
                {
                    UpdateHead(Tail);
                }
                CancelSelection();
                return;
            }
            UpdateTail(selection);
            Head.ShiftOneCharacterToTheRight();
        }

        public void ShiftHeadOneWordToTheRight(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            UpdateTail(selection);
            Head.ShiftOneWordToTheRight(syntaxHighlighter);
        }

        internal void ShiftHeadOneWordToTheLeft(ISyntaxHighlighter syntaxHighlighter, bool selection = false)
        {
            UpdateTail(selection);
            Head.ShiftOneWordToTheLeft(syntaxHighlighter);
        }

        public void ShiftHeadToEndOfLine(bool selection = false)
        {
            UpdateTail(selection);
            Head.ShiftToEndOfLine();
        }

        public void ShiftHeadToHome(bool selection = false)
        {
            UpdateTail(selection);
            Head.ShiftToHome();
        }

        public void RemoveTabFromBeforeActivePosition(List<UndoRedoAction> actionBuilder) => RemoveTabFromBeforePosition(Head, actionBuilder);

        private void RemoveTabFromBeforePosition(Cursor position, List<UndoRedoAction> actionBuilder)
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

        public void IncreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    start.Line.Value.IncreaseIndentAtPosition(0, out _);
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
                Head.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
            }
        }

        public void IncreaseIndentAtActivePosition()
        {
            Head.Line.Value.IncreaseIndentAtPosition(Head.ColumnNumber, out int shiftAmount);
            Head.ColumnNumber += shiftAmount;
        }

        public void DecreaseIndentAtActivePosition()
        {
            Head.Line.Value.DecreaseIndentAtPosition(Head.ColumnNumber, out int shiftAmount);
            Head.ColumnNumber -= shiftAmount;
        }

        public void DecreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    start.Line.Value.DecreaseIndentAtPosition(start.Line.Value.FirstNonWhiteSpaceIndex, out _);
                    if (!start.ShiftDownOneLine())
                    {
                        break;
                    }
                }
                Tail.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
                Head.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
            }
        }
    }
}
