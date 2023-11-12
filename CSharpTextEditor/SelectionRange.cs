using System;
using System.Collections.Generic;
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
        public Cursor? Tail { get; private set; }

        /// <summary>
        /// The "active" position of the cursor. Characters will be removed/added, etc from this position
        /// </summary>
        public Cursor Head { get; }

        public SelectionRange(LinkedListNode<SourceCodeLine> initialLine, int initialLineNumber, int initialColumnNumber)
            :this(null, new Cursor(initialLine, initialColumnNumber, initialLineNumber))
        {
        }

        public SelectionRange(Cursor? tail, Cursor head)
        {
            Tail = tail;
            Head = head;
        }

        public void RemoveSelected()
        {
            (Cursor first, Cursor last) = GetOrderedCursors(false);
            while (last > first)
            {
                RemoveCharacterBeforeHead(last);
            }
            CancelSelection();
        }

        private void RemoveRange(Cursor start, Cursor end)
        {
            (Cursor first, Cursor last) = GetOrderedCursors(start, end);
            while (last > first)
            {
                RemoveCharacterBeforeHead(last);
            }
            CancelSelection();
        }

        public void RemoveCharacterBeforeHead()
        {
            if (IsRangeSelected())
            {
                RemoveSelected();
            }
            else
            {
                RemoveCharacterBeforeHead(Head);
            }
        }

        private void RemoveCharacterBeforeHead(Cursor head)
        {
            if (head.Line.Value.RemoveCharacterBefore(head.ColumnNumber))
            {
                head.ColumnNumber--;
            }
            else if (head.Line.Previous != null)
            {
                LinkedListNode<SourceCodeLine> oldCurrent = head.Line;

                int columnNumber = head.Line.Previous.Value.Text.Length;
                int lineNumber = head.LineNumber - 1;
                head.Line = head.Line.Previous;
                head.ColumnNumber = columnNumber;
                head.LineNumber = lineNumber;
                head.Line.Value.Text += oldCurrent.Value.Text;
                head.Line.List.Remove(oldCurrent);
            }
            head.ResetMaxColumnNumber();
        }

        public void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelected();
            }
            RemoveWordBeforePosition(Head, syntaxHighlighter);
        }

        private void RemoveWordBeforePosition(Cursor position, ISyntaxHighlighter syntaxHighlighter)
        {
            Cursor startOfPreviousWord = position.Clone();
            startOfPreviousWord.ShiftOneWordToTheLeft(syntaxHighlighter);
            RemoveRange(startOfPreviousWord, position);
        }

        public void RemoveCharacterAfterActivePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelected();
            }
            else
            {
                if (Head.Line.Value.RemoveCharacterAfter(Head.ColumnNumber))
                {

                }
                else if (Head.Line.Next != null)
                {
                    Head.Line.Value.AppendText(Head.Line.Next.Value.Text);
                    Head.Line.List.Remove(Head.Line.Next);
                }
            }
            Head.ResetMaxColumnNumber();
        }

        public void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelected();
            }
            RemoveWordAfterPosition(Head, syntaxHighlighter);
        }

        private void RemoveWordAfterPosition(Cursor position, ISyntaxHighlighter syntaxHighlighter)
        {
            Cursor startOfNextWord = position.Clone();
            startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
            RemoveRange(position, startOfNextWord);
        }

        public void InsertLineBreakAtActivePosition(SourceCode sourceCode, ISpecialCharacterHandler? specialCharacterHandler = null)
        {
            if (Tail != null)
            {
                RemoveSelected();
            }
            Cursor head = Head;
            string newLineContents = string.Empty;
            if (!head.AtEndOfLine())
            {
                newLineContents = head.Line.Value.GetStringAfterPosition(head.ColumnNumber);
                head.Line.Value.Text = head.Line.Value.GetStringBeforePosition(head.ColumnNumber);
            }
            LinkedListNode<SourceCodeLine> newLine = head.Line.List.AddAfter(head.Line, new SourceCodeLine(newLineContents));
            head.Line = newLine;
            head.LineNumber++;
            head.ColumnNumber = 0;
            head.ResetMaxColumnNumber();

            specialCharacterHandler?.HandleLineBreakInserted(sourceCode, head);
        }

        public void InsertCharacterAtActivePosition(char character, SourceCode sourceCode, ISpecialCharacterHandler? specialCharacterHandler)
        {
            if (Tail != null)
            {
                RemoveSelected();
            }
            if (character == '\t')
            {
                InsertStringAtActivePosition(SourceCode.TAB_REPLACEMENT, sourceCode, specialCharacterHandler);
                return;
            }
            specialCharacterHandler?.HandleCharacterInserting(character, sourceCode);
            Head.InsertCharacter(character);
        }

        public void InsertStringAtActivePosition(string text, SourceCode sourceCode, ISpecialCharacterHandler? specialCharacterHandler)
        {
            if (Tail != null)
            {
                RemoveSelected();
            }
            text = text.Replace("\t", SourceCode.TAB_REPLACEMENT);
            using (StringReader sr = new StringReader(text))
            {
                string? currentLine = sr.ReadLine();
                while (currentLine != null)
                {
                    Head.InsertText(currentLine);
                    currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        InsertLineBreakAtActivePosition(sourceCode, specialCharacterHandler);
                    }
                }
            }
            Head.ResetMaxColumnNumber();
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
                    sb.Append(start.Line.Value.Text[start.ColumnNumber]);
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
            UpdateTail(selection);
            Head.ShiftOneCharacterToTheLeft();
        }

        public void ShiftHeadToTheRight(bool selection = false)
        {
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

        public void ShiftHeadToStartOfLine(bool selection = false)
        {
            UpdateTail(selection);
            Head.ShiftToStartOfLine();
        }

        public void RemoveTabFromBeforeActivePosition() => RemoveTabFromBeforePosition(Head);

        private void RemoveTabFromBeforePosition(Cursor position)
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
                RemoveRange(otherEnd, position);
            }
        }



        public void IncreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    start.Line.Value.Text = SourceCode.TAB_REPLACEMENT + start.GetLineValue();
                    start.ShiftDownOneLine();
                }
                Tail.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
                Head.ColumnNumber += SourceCode.TAB_REPLACEMENT.Length;
            }
        }

        public void DecreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    string lineValue = start.GetLineValue();
                    if (lineValue.Length < SourceCode.TAB_REPLACEMENT.Length)
                    {
                        int firstNonWhiteSpaceCharacter = 0;
                        for (int i = 0; i < lineValue.Length; i++)
                        {
                            if (!char.IsWhiteSpace(lineValue[i]))
                            {
                                firstNonWhiteSpaceCharacter = i;
                                break;
                            }
                        }
                        if (firstNonWhiteSpaceCharacter > 0)
                        {
                            start.Line.Value.Text = lineValue.Substring(firstNonWhiteSpaceCharacter);
                        }
                    }
                    else if (lineValue.Substring(0, SourceCode.TAB_REPLACEMENT.Length) == SourceCode.TAB_REPLACEMENT)
                    {
                        start.Line.Value.Text = lineValue.Substring(SourceCode.TAB_REPLACEMENT.Length);
                    }
                    start.ShiftDownOneLine();
                }
                Tail.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
                Head.ColumnNumber -= Math.Max(0, SourceCode.TAB_REPLACEMENT.Length);
            }
        }
    }
}
