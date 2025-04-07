using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
using CSharpTextEditor.Utility;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View
{
    public class ViewManager
    {
        public const int LEFT_MARGIN = 6;
        public const int CURSOR_OFFSET = 0;

        private int verticalScrollPositionPX;
        private int horizontalScrollPositionPX;
        private readonly IClipboard clipboard;

        internal SourceCode SourceCode { get; }

        public SyntaxPalette SyntaxPalette { get; set; }

        public SyntaxHighlightingCollection? CurrentHighlighting { get; set; }

        public int CharacterWidth { get; set; }

        public int LineWidth { get; set; }

        public int VerticalScrollPositionPX
        {
            get => verticalScrollPositionPX;
            set
            {
                var newPosition = Maths.Clamp(0, value, GetMaxVerticalScrollPosition());
                if (newPosition != verticalScrollPositionPX)
                {
                    verticalScrollPositionPX = newPosition;
                    VerticalScrollChanged?.Invoke();
                }
            }
        }

        public int HorizontalScrollPositionPX
        {
            get => horizontalScrollPositionPX;
            set
            {
                var newPosition = Maths.Clamp(0, value, GetMaxHorizontalScrollPosition());
                if (newPosition != horizontalScrollPositionPX)
                {
                    horizontalScrollPositionPX = newPosition;
                    HorizontalScrollChanged?.Invoke();
                }
            }
        }

        internal ISpecialCharacterHandler? SpecialCharacterHandler { get; set; }

        internal ISyntaxHighlighter? SyntaxHighlighter { get; set; }

        public event Action? VerticalScrollChanged;

        public event Action? HorizontalScrollChanged;

        internal ViewManager(SourceCode sourceCode, IClipboard clipboard)
        {
            SourceCode = sourceCode;
            this.clipboard = clipboard;
        }

        internal void Draw(ICanvas canvas, DrawSettings settings)
        {
            canvas.Clear(SyntaxPalette.BackColour);
            int lineIndex = 0;

            string selectedText = string.Empty;
            if (SourceCode.SelectionRangeCollection.Count == 1
                && !SourceCode.SelectionCoversMultipleLines())
            {
                selectedText = SourceCode.GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    selectedText = string.Empty;
                }
            }

            int x = LEFT_MARGIN + GetGutterWidth() - HorizontalScrollPositionPX;
            foreach (string s in SourceCode.Lines)
            {
                DrawSelectionRectangleOnLine(canvas, settings.Focused, lineIndex, selectedText, s);
                int y = GetYCoordinateFromLineIndex(lineIndex);
                if (y > -LineWidth
                    && y < canvas.Size.Height)
                {
                    DrawingHelper.DrawTextLine(canvas, lineIndex, s, x, y, CurrentHighlighting?.Highlightings, SyntaxPalette);
                }
                lineIndex++;
            }

            DrawErrorSquiggles(canvas);

            if (settings.Focused
                && settings.CursorBlinkOn)
            {
                DrawCursors(canvas);
            }
            DrawLeftGutter(canvas);
        }

        public int GetMaxHorizontalScrollPosition() => SourceCode.Lines.Max(x => x.Length) * CharacterWidth;

        public int GetMaxVerticalScrollPosition() => (SourceCode.LineCount - 1) * LineWidth;

        public void ScrollView(int numberOfLines) => VerticalScrollPositionPX += numberOfLines * LineWidth;

        public void EnsureActivePositionInView(Size viewSize)
        {
            EnsureVerticalActivePositionInView(viewSize);
            EnsureHorizontalActivePositionInView(viewSize);
        }

        private void EnsureVerticalActivePositionInView(Size viewSize)
        {
            int activeLine = SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int minLineInView = VerticalScrollPositionPX / LineWidth;
            int maxLineInView = (VerticalScrollPositionPX + viewSize.Height - LineWidth) / LineWidth;
            if (activeLine > maxLineInView)
            {
                VerticalScrollPositionPX = activeLine * LineWidth - viewSize.Height + LineWidth;
            }
            else if (activeLine < minLineInView)
            {
                VerticalScrollPositionPX = activeLine * LineWidth;
            }
        }

        private void EnsureHorizontalActivePositionInView(Size viewSize)
        {
            int characterWidth = CharacterWidth;

            int activeColumn = SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            int minColumnInView = HorizontalScrollPositionPX / characterWidth;
            int maxColumnInView = (HorizontalScrollPositionPX + viewSize.Width - characterWidth - GetGutterWidth() - LEFT_MARGIN) / characterWidth;
            if (activeColumn > maxColumnInView)
            {
                HorizontalScrollPositionPX = activeColumn * characterWidth - viewSize.Width + GetGutterWidth() + LEFT_MARGIN + characterWidth;
            }
            else if (activeColumn < minColumnInView)
            {
                HorizontalScrollPositionPX = Math.Max(0, activeColumn - 6) * characterWidth;
            }
        }

        public SourceCodePosition GetPositionFromScreenPoint(Point point)
        {
            int line = (point.Y + VerticalScrollPositionPX) / LineWidth;
            int column = GetColumnFromGlobalX(point.X + HorizontalScrollPositionPX - GetGutterWidth() - LEFT_MARGIN);
            return new SourceCodePosition(Math.Max(0, line), Math.Max(0, column));
        }

        private int GetColumnFromGlobalX(int globalX)
        {
            // TODO: This only works for monospaced fonts
            return globalX / CharacterWidth;
        }

        private void DrawCursors(ICanvas canvas)
        {
            bool multicaret = SourceCode.SelectionRangeCollection.Count > 1;
            int count = 0;
            foreach (SelectionRange range in SourceCode.SelectionRangeCollection)
            {
                Color colour = GetCaretColour(SyntaxPalette, multicaret, count);
                Cursor position = range.Head;
                int x = GetXCoordinateFromColumnIndex(position.ColumnNumber);
                int y = GetYCoordinateFromLineIndex(position.LineNumber);
                if (SourceCode.OvertypeEnabled
                    && !range.IsRangeSelected()
                    && !position.AtEndOfLine())
                {
                    canvas.FillRectangle(Color.FromArgb(96, colour),
                        new Rectangle(CURSOR_OFFSET + x, y, CharacterWidth, LineWidth));
                }
                else
                {
                    canvas.DrawLine(colour,
                        new Point(CURSOR_OFFSET + x, y),
                        new Point(CURSOR_OFFSET + x, y + LineWidth));
                }
                count++;
            }
        }

        private Color GetCaretColour(SyntaxPalette palette, bool multicaret, int index)
        {
            if (multicaret)
            {
                if (index == 0)
                {
                    return SyntaxPalette.MultiCaretPrimaryCursorColour;
                }
                return SyntaxPalette.MultiCaretSecondaryCursorColour;
            }
            return SyntaxPalette.CursorColour;
        }

        private void DrawErrorSquiggles(ICanvas canvas)
        {
            if (CurrentHighlighting != null)
            {
                foreach (SyntaxDiagnostic diagnostic in CurrentHighlighting.Diagnostics)
                {
                    var start = diagnostic.Start;
                    var end = diagnostic.End;

                    int startColumn = start.ColumnNumber;
                    for (int errorLine = start.LineNumber; errorLine <= end.LineNumber; errorLine++)
                    {
                        int endColumn = errorLine == end.LineNumber ? end.ColumnNumber : SourceCode.Lines.ElementAt(errorLine).Length;
                        int y = errorLine * LineWidth + LineWidth - VerticalScrollPositionPX;
                        int thisEndColumn = endColumn;
                        if (startColumn == endColumn)
                        {
                            thisEndColumn++;
                        }
                        int startX = GetXCoordinateFromColumnIndex(startColumn);
                        int endX = GetXCoordinateFromColumnIndex(thisEndColumn);
                        canvas.DrawSquigglyLine(Color.Red, startX, endX, y);
                        startColumn = 0;
                    }
                }
            }
        }

        private void DrawSelectionRectangleOnLine(ICanvas canvas, bool focused, int lineIndex, string selectedText, string line)
        {
            bool selectionRectangleDrawn = false;
            foreach (SelectionRange range in SourceCode.SelectionRangeCollection)
            {
                bool rangeSelected = range.IsRangeSelected();
                (Cursor startCursor, Cursor endCursor) = range.GetOrderedCursors();
                int selectionEndLine = endCursor.LineNumber;
                int selectionStartLine = startCursor.LineNumber;
                if (selectionStartLine > selectionEndLine)
                {
                    (selectionStartLine, selectionEndLine) = (selectionEndLine, selectionStartLine);
                }
                if (rangeSelected
                    && lineIndex >= selectionStartLine
                    && lineIndex <= selectionEndLine)
                {
                    Color colour = focused ? SyntaxPalette.SelectionColour : SyntaxPalette.DefocusedSelectionColour;
                    canvas.FillRectangle(colour, GetLineSelectionRectangle(range, lineIndex, line.Length));
                    selectionRectangleDrawn = true;
                }
            }
            if (!selectionRectangleDrawn
                && !string.IsNullOrEmpty(selectedText))
            {
                int index = 0;
                do
                {
                    index = line.IndexOf(selectedText, index, StringComparison.CurrentCultureIgnoreCase);
                    if (index != -1)
                    {
                        canvas.FillRectangle(SyntaxPalette.DefocusedSelectionColour, GetLineRectangle(index, index + selectedText.Length, lineIndex));
                        index++;
                    }
                } while (index != -1);
            }
        }

        private void DrawLeftGutter(ICanvas canvas)
        {
            Color gutterColour = Color.Gray;

            int gutterWidth = GetGutterWidth();
            
            int width = gutterWidth;
            canvas.FillRectangle(SyntaxPalette.BackColour, new Rectangle(0, 0, width, canvas.Size.Height));
            
            int lastLineCoordinate = GetYCoordinateFromLineIndex(SourceCode.LineCount);
            canvas.DrawLine(gutterColour, new Point(gutterWidth, 0), new Point(gutterWidth, lastLineCoordinate));
            if (CurrentHighlighting != null)
            {
                foreach (var block in CurrentHighlighting.BlockLines)
                {
                    int startLineCoordinate = GetYCoordinateFromLineIndex(block.Item1);
                    int endLineCoordinate = GetYCoordinateFromLineIndex(block.Item2);
                    canvas.DrawLine(gutterColour, new Point(gutterWidth, startLineCoordinate), new Point(gutterWidth + LEFT_MARGIN - 2, startLineCoordinate));
                    canvas.DrawLine(gutterColour, new Point(gutterWidth, endLineCoordinate), new Point(gutterWidth + LEFT_MARGIN - 2, endLineCoordinate));
                }
            }
            canvas.DrawLine(gutterColour, new Point(gutterWidth, lastLineCoordinate), new Point(gutterWidth + LEFT_MARGIN - 2, lastLineCoordinate));
            int lineNumber = 0;

            foreach (string s in SourceCode.Lines)
            {
                int y = GetYCoordinateFromLineIndex(lineNumber);

                lineNumber++;

                canvas.DrawText(lineNumber.ToString(), gutterColour, new Rectangle(0, y, gutterWidth, y + LineWidth), rightAlign: true);
            }
        }

        private Rectangle GetLineSelectionRectangle(SelectionRange range, int lineNumber, int lineCharacterLength)
        {
            (var start, var end) = range.GetOrderedCursors();

            int selectionEndLine = end.LineNumber;
            int selectionStartLine = start.LineNumber;

            int startCharacterIndex;
            int endCharacterIndex;
            if (lineNumber == selectionStartLine)
            {
                startCharacterIndex = start.ColumnNumber;
                endCharacterIndex = lineNumber == selectionEndLine ? end.ColumnNumber : lineCharacterLength;
            }
            else if (lineNumber == selectionEndLine)
            {
                startCharacterIndex = 0;
                endCharacterIndex = end.ColumnNumber;
            }
            else
            {
                startCharacterIndex = 0;
                endCharacterIndex = lineCharacterLength;
            }
            if (startCharacterIndex > endCharacterIndex)
            {
                (startCharacterIndex, endCharacterIndex) = (endCharacterIndex, startCharacterIndex);
            }
            if (startCharacterIndex == endCharacterIndex)
            {
                endCharacterIndex++;
            }
            int y = GetYCoordinateFromLineIndex(lineNumber);
            return GetLineRectangle(startCharacterIndex, endCharacterIndex, lineNumber);
        }

        private Rectangle GetLineRectangle(int startColumn, int endColumn, int lineNumber)
        {
            int y = GetYCoordinateFromLineIndex(lineNumber);
            return Rectangle.FromLTRB(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(startColumn),
                                      y,
                                      CURSOR_OFFSET + GetXCoordinateFromColumnIndex(endColumn),
                                      y + LineWidth);
        }

        public int GetXCoordinateFromColumnIndex(int columnIndex)
        {
            // TODO: This only works for monospaced fonts
            return LEFT_MARGIN + GetGutterWidth() + columnIndex * CharacterWidth - HorizontalScrollPositionPX;
        }

        public int GetYCoordinateFromLineIndex(int lineIndex)
        {
            return lineIndex * LineWidth - VerticalScrollPositionPX;
        }

        public int GetGutterWidth()
        {
            int digitsInLineCount = NumberOfDigits(SourceCode.LineCount);
            return Math.Max(4, 1 + digitsInLineCount) * CharacterWidth;
        }

        private int NumberOfDigits(int value)
        {
            if (value == 0)
            {
                return 1;
            }
            value = Math.Abs(value);
            int count = 0;
            while (value > 0)
            {
                value /= 10;
                count++;
            }
            return count;
        }

        public void RemoveLineAtActivePosition() => SourceCode.RemoveLineAtActivePosition();

        public void SwapLineUpAtActivePosition() => SourceCode.SwapLinesUpAtActivePosition();

        public void SwapLineDownAtActivePosition() => SourceCode.SwapLinesDownAtActivePosition();

        public void SelectAll() => SourceCode.SelectAll();

        public void DuplicateSelection() => SourceCode.DuplicateSelection();

        public void SelectionToLowerCase() => SourceCode.SelectionToLowerCase();

        public void SelectionToUpperCase() => SourceCode.SelectionToUpperCase();

        public void Undo() => SourceCode.Undo();

        public void Redo() => SourceCode.Redo();

        public void RemoveWordAfterActivePosition() => SourceCode.RemoveWordAfterActivePosition(SyntaxHighlighter);

        public void RemoveWordBeforeActivePosition() => SourceCode.RemoveWordBeforeActivePosition(SyntaxHighlighter);

        public void GoToLastPosition() => SourceCode.SetActivePosition(SourceCode.LineCount, SourceCode.Lines.Last().Length);

        public void GoToFirstPosition() => SourceCode.SetActivePosition(0, 0);

        public void ShiftActivePositionOneWordToTheRight(bool select) => SourceCode.ShiftHeadOneWordToTheRight(SyntaxHighlighter, select);

        public void ShiftActivePositionOneWordToTheLeft(bool select) => SourceCode.ShiftHeadOneWordToTheLeft(SyntaxHighlighter, select);

        public void PasteFromClipboard() => SourceCode.InsertStringAtActivePosition(clipboard.GetText());

        public void CopySelectedToClipboard()
        {
            string selectedTextForCopy = SourceCode.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedTextForCopy))
            {
                clipboard.SetText(selectedTextForCopy);
            }
        }

        public void CutSelectedToClipboard()
        {
            string selectedTextForCut = SourceCode.GetSelectedText();
            SourceCode.RemoveSelectedRange();
            if (!string.IsNullOrEmpty(selectedTextForCut))
            {
                clipboard.SetText(selectedTextForCut);
            }
        }
    }
}
