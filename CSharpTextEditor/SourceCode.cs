using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal class SourceCode
    {
        public const string TAB_REPLACEMENT = "    ";

        private readonly LinkedList<SourceCodeLine> _lines;

        public SelectionRange SelectionRange { get; }

        public Cursor Head => SelectionRange.Head;

        public string Text
        {
            get => string.Join(Environment.NewLine, Lines);
            set => SetLinesFromText(value);
        }

        public IReadOnlyCollection<string> Lines => _lines.Select(x => x.Text).ToArray();

        public SourceCode()
            :this(string.Empty)
        { 
        }

        public SourceCode(string text)
        {
            _lines = new LinkedList<SourceCodeLine>(new[] { new SourceCodeLine(string.Empty) });
            if (!string.IsNullOrEmpty(text))
            {
                SetLinesFromText(text);
            }
            if (_lines.Last == null)
            {
                throw new Exception("Something has gone wrong. _lines should always have at least one value here");
            }
            SelectionRange = new SelectionRange(_lines.Last, _lines.Count - 1, _lines.Last.Value.Text.Length);
        }

        private void SetLinesFromText(string text)
        {
            _lines.Clear();
            foreach (string textLine in text.Replace("\t", SourceCode.TAB_REPLACEMENT).Split(Environment.NewLine))
            {
                _lines.AddLast(new SourceCodeLine(textLine));
            }
        }

        public bool IsRangeSelected()
        {
            return SelectionRange.IsRangeSelected();
        }

        public void RemoveSelectedRange()
        {
            (Cursor start, Cursor end) = SelectionRange.GetOrderedCursors();
            RemoveRange(start, end);
        }

        private void RemoveRange(Cursor start, Cursor end)
        {
            (Cursor first, Cursor last) = SelectionRange.GetOrderedCursors(start, end);
            while (last > first)
            {
                RemoveCharacterBeforePosition(last);
            }
            SelectionRange.CancelSelection();
        }

        public void RemoveCharacterBeforeActivePosition()
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            else
            {
                RemoveCharacterBeforePosition(SelectionRange.Head);
            }
        }

        public void RemoveTabFromBeforeActivePosition() => RemoveTabFromBeforePosition(SelectionRange.Head);

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
                otherEnd.ColumnNumber = Math.Max(0, otherEnd.ColumnNumber - TAB_REPLACEMENT.Length);
                RemoveRange(otherEnd, position);
            }
        }

        private void RemoveCharacterBeforePosition(Cursor position)
        {
            if (position.Line.Value.RemoveCharacterBefore(position.ColumnNumber))
            {
                position.ColumnNumber--;
            }
            else if (position.Line.Previous != null)
            {
                LinkedListNode<SourceCodeLine> oldCurrent = position.Line;

                int columnNumber = position.Line.Previous.Value.Text.Length;
                int lineNumber = position.LineNumber - 1;
                position.Line = position.Line.Previous;
                position.ColumnNumber = columnNumber;
                position.LineNumber = lineNumber;
                position.Line.Value.Text += oldCurrent.Value.Text;
                _lines.Remove(oldCurrent);
            }
            SelectionRange.Head.ResetMaxColumnNumber();
        }

        public void RemoveWordBeforeActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            RemoveWordBeforePosition(SelectionRange.Head, syntaxHighlighter);
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
                RemoveSelectedRange();
            }
            else
            {
                if (SelectionRange.Head.Line.Value.RemoveCharacterAfter(SelectionRange.Head.ColumnNumber))
                {
                    
                }
                else if (SelectionRange.Head.Line.Next != null)
                {
                    SelectionRange.Head.Line.Value.AppendText(SelectionRange.Head.Line.Next.Value.Text);
                    _lines.Remove(SelectionRange.Head.Line.Next);
                }
            }
            SelectionRange.Head.ResetMaxColumnNumber();
        }

        public void RemoveWordAfterActivePosition(ISyntaxHighlighter syntaxHighlighter)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            RemoveWordAfterPosition(SelectionRange.Head, syntaxHighlighter);
        }

        private void RemoveWordAfterPosition(Cursor position, ISyntaxHighlighter syntaxHighlighter)
        {
            Cursor startOfNextWord = position.Clone();
            startOfNextWord.ShiftOneWordToTheRight(syntaxHighlighter);
            RemoveRange(position, startOfNextWord);
        }

        public void IncreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = SelectionRange.GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    start.Line.Value.Text = TAB_REPLACEMENT + start.GetLineValue();
                    start.ShiftDownOneLine();
                }
                SelectionRange.Tail.ColumnNumber += TAB_REPLACEMENT.Length;
                SelectionRange.Head.ColumnNumber += TAB_REPLACEMENT.Length;
            }
        }

        public void DecreaseIndentOnSelectedLines()
        {
            if (IsRangeSelected())
            {
                (Cursor start, Cursor end) = SelectionRange.GetOrderedCursors();
                while (start.LineNumber <= end.LineNumber)
                {
                    string lineValue = start.GetLineValue();
                    if (lineValue.Length < TAB_REPLACEMENT.Length)
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
                    else if (lineValue.Substring(0, TAB_REPLACEMENT.Length) == TAB_REPLACEMENT)
                    {
                        start.Line.Value.Text = lineValue.Substring(TAB_REPLACEMENT.Length);
                    }
                    start.ShiftDownOneLine();
                }
                SelectionRange.Tail.ColumnNumber -= Math.Max(0, TAB_REPLACEMENT.Length);
                SelectionRange.Head.ColumnNumber -= Math.Max(0, TAB_REPLACEMENT.Length);
            }
        }

        public void InsertLineBreakAtActivePosition(ISpecialCharacterHandler? specialCharacterHandler = null)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            Cursor head = SelectionRange.Head;
            string newLineContents = string.Empty;
            if (!head.AtEndOfLine())
            {
                newLineContents = head.Line.Value.GetStringAfterPosition(head.ColumnNumber);
                head.Line.Value.Text = head.Line.Value.GetStringBeforePosition(head.ColumnNumber);
            }
            LinkedListNode<SourceCodeLine> newLine = _lines.AddAfter(head.Line, new SourceCodeLine(newLineContents));
            head.Line = newLine;
            head.LineNumber++;
            head.ColumnNumber = 0;
            head.ResetMaxColumnNumber();

            specialCharacterHandler?.HandleLineBreakInserted(this, head);
        }

        public void InsertCharacterAtActivePosition(char character, ISpecialCharacterHandler? specialCharacterHandler)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            if (character == '\t')
            {
                InsertStringAtActivePosition(TAB_REPLACEMENT);
                return;
            }
            specialCharacterHandler?.HandleCharacterInserting(character, this);
            SelectionRange.Head.InsertCharacter(character);
        }

        public void InsertStringAtActivePosition(string text)
        {
            if (IsRangeSelected())
            {
                RemoveSelectedRange();
            }
            text = text.Replace("\t", SourceCode.TAB_REPLACEMENT);
            using (StringReader sr = new StringReader(text))
            {
                string? currentLine = sr.ReadLine();
                while (currentLine != null)
                {
                    SelectionRange.Head.InsertText(currentLine);
                    currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        InsertLineBreakAtActivePosition();
                    }
                }
            }
            SelectionRange.Head.ResetMaxColumnNumber();
        }

        public void SetActivePosition(int lineNumber, int columnNumber)
        {
            SelectionRange.CancelSelection();
            Cursor position = GetPosition(lineNumber, columnNumber);
            SelectionRange.UpdateHead(position.Line, position.LineNumber, position.ColumnNumber);
        }

        private Cursor GetPosition(int lineNumber, int columnNumber)
        {
            var current = _lines.First;
            int count = 0;
            while (current != null)
            {
                if (count++ == lineNumber)
                {
                    return new Cursor(current, Math.Min(columnNumber, current.Value.Text.Length), lineNumber);
                }
                current = current.Next;
            }
            if (_lines.Last != null)
            {
                return new Cursor(_lines.Last, Math.Min(columnNumber, _lines.Last.Value.Text.Length), _lines.Count - 1);
            }
            throw new Exception("Couldn't get position");
        }

        public void SelectRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine == endLine
                && startColumn == endColumn)
            {
                SetActivePosition(endLine, endColumn);
                return;
            }

            Cursor start = GetPosition(startLine, startColumn);
            Cursor end = GetPosition(endLine, endColumn);
            SelectionRange.SelectRange(start, end);
        }

        public void SelectTokenAtPosition(SourceCodePosition position, ISyntaxHighlighter syntaxHighlighter)
        {
            int previousStart = 0;
            Cursor selectionPosition = GetPosition(position.LineNumber, position.ColumnNumber);
            foreach ((int tokenStart, int tokenEnd) in syntaxHighlighter.GetSpansFromTextLine(selectionPosition.GetLineValue()))
            {
                if (position.ColumnNumber <= tokenStart)
                {
                    SelectRange(position.LineNumber, previousStart, position.LineNumber, tokenStart);
                    break;
                }
                if (tokenStart <= position.ColumnNumber && tokenEnd >= position.ColumnNumber)
                {
                    SelectRange(position.LineNumber, tokenStart, position.LineNumber, tokenEnd);
                    break;
                }
                previousStart = tokenEnd;
            }
        }

        public void SelectAll()
        {
            if (_lines.First != null
                && _lines.Last != null)
            {
                SelectionRange.SelectRange(new Cursor(_lines.First, 0, 0), new Cursor(_lines.Last, _lines.Last.Value.Text.Length, _lines.Count - 1));
            }
        }
    }
}
