using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.View
{

    internal class ViewManager
    {
        public const int LEFT_MARGIN = 6;
        public const int CURSOR_OFFSET = 0;

        public SourceCode SourceCode { get; }

        public SyntaxPalette SyntaxPalette { get; set; }

        public SyntaxHighlightingCollection? Highlighting { get; set; }

        public int CharacterWidth { get; set; }

        public int LineWidth { get; set; }

        public int VerticalScrollPositionPX { get; set; }

        public int HorizontalScrollPositionPX { get; set; }

        public ViewManager(SourceCode sourceCode)
        {
            SourceCode = sourceCode;
        }

        internal void Draw(ICanvas canvas, bool focused)
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

            foreach (string s in SourceCode.Lines)
            {
                DrawSelectionRectangleOnLine(canvas, focused, lineIndex, selectedText, s);
                int y = GetYCoordinateFromLineIndex(lineIndex);
                if (y > -LineWidth
                    && y < canvas.Size.Height)
                {
                    DrawingHelper.DrawLine(canvas, lineIndex, s, y, Highlighting?.Highlightings, GetXCoordinateFromColumnIndex, SyntaxPalette);
                }
                lineIndex++;
            }

            DrawErrorSquiggles(canvas);

            if (focused)
            {
                DrawCursors(canvas);
            }
            DrawLeftGutter(canvas);
        }

        private void DrawCursors(ICanvas canvas)
        {
            foreach (SelectionRange range in SourceCode.SelectionRangeCollection)
            {
                Cursor position = range.Head;
                canvas.DrawLine(SyntaxPalette.CursorColour,
                    new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber)),
                    new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber) + LineWidth));
            }
        }

        private void DrawErrorSquiggles(ICanvas canvas)
        {
            if (Highlighting != null)
            {
                foreach ((SourceCodePosition start, SourceCodePosition end, string _) in Highlighting.Errors)
                {
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
                        /*using (Pen p = new Pen(Color.Red))
                        {
                            p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            DrawSquigglyLine(canvas, p, startX, endX, y);
                        }*/
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
            if (Highlighting != null)
            {
                foreach (var block in Highlighting.BlockLines)
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
            return Rectangle.FromLTRB(ViewManager.CURSOR_OFFSET + GetXCoordinateFromColumnIndex(startColumn),
                                      y,
                                      ViewManager.CURSOR_OFFSET + GetXCoordinateFromColumnIndex(endColumn),
                                      y + LineWidth);
        }

        public int GetXCoordinateFromColumnIndex(int columnIndex)
        {
            return ViewManager.LEFT_MARGIN + GetGutterWidth() + columnIndex * CharacterWidth - HorizontalScrollPositionPX;
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
    }
}
