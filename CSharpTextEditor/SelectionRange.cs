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
        {
            Head = new Cursor(initialLine, initialColumnNumber, initialLineNumber);
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

        public (Cursor first, Cursor last) GetOrderedCursors()
        {
            return GetOrderedCursors((Tail ?? Head).Clone(), Head.Clone());
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
            Head.ColumnNumber = newLineIndex;
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
    }
}
