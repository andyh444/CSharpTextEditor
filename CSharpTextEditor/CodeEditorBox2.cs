using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox2 : UserControl
    {
        private const int LINE_WIDTH = 20;
        private const int CURSOR_OFFSET = 2;

        private readonly SourceCode _sourceCode;
        private readonly int _characterWidth;
        private int? dragLineStart = null;
        private int? dragColumnStart = null;
        private int verticalScrollPositionPX;
        private int horizontalScrollPositionPX;
        private SyntaxHighlightingCollection? _highlighting;
        private ISpecialCharacterHandler _specialCharacterHandler;

        [Browsable(true)]
        public new string Text { get; set; }

        public CodeEditorBox2()
        {
            InitializeComponent();
            _sourceCode = new SourceCode(Text ?? string.Empty);
            _characterWidth = TextRenderer.MeasureText("A", Font).Width / 2;
            verticalScrollPositionPX = 0;
            horizontalScrollPositionPX = 0;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;
            _highlighting = null;
            _specialCharacterHandler = new CSharpSpecialCharacterHandler();
        }

        private void UpdateSyntaxHighlighting()
        {
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter(charIndex => SourceCodePosition.FromCharacterIndex(charIndex, _sourceCode.Lines));
            _highlighting = highlighter.GetHighlightings(_sourceCode.Text, SyntaxPalette.GetLightModePalette());
        }

        private void CodeEditorBox2_MouseWheel(object? sender, MouseEventArgs e)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            verticalScrollPositionPX = Math.Clamp(verticalScrollPositionPX - 3 * LINE_WIDTH * Math.Sign(e.Delta), 0, maxScrollPosition);
            vScrollBar1.Value = maxScrollPosition == 0 ? 0 : (vScrollBar1.Maximum * verticalScrollPositionPX) / maxScrollPosition;
            Refresh();
        }

        private int GetMaxHorizontalScrollPosition()
        {
            return _sourceCode.Lines.Max(x => x.Length) * _characterWidth;
        }

        private int GetMaxVerticalScrollPosition()
        {
            return (_sourceCode.Lines.Count - 1) * LINE_WIDTH;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            verticalScrollPositionPX = (vScrollBar1.Value * maxScrollPosition) / vScrollBar1.Maximum;
            Refresh();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            horizontalScrollPositionPX = (hScrollBar1.Value * maxScrollPosition) / hScrollBar1.Maximum;
            Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // not strictly part of drawing, but close enough
            vScrollBar1.Maximum = GetMaxVerticalScrollPosition() / LINE_WIDTH;
            hScrollBar1.Maximum = GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            e.Graphics.Clear(Color.White);
            int line = 0;

            bool rangeSelected = _sourceCode.SelectionStart != null;
            int selectionEndLine = _sourceCode.SelectionEnd.LineNumber;
            int selectionStartLine = _sourceCode.SelectionStart?.LineNumber ?? selectionEndLine;
            if (selectionStartLine > selectionEndLine)
            {
                (selectionStartLine, selectionEndLine) = (selectionEndLine, selectionStartLine);
            }
            foreach (string s in _sourceCode.Lines)
            {
                if (rangeSelected
                    && line >= selectionStartLine
                    && line <= selectionEndLine)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, GetLineSelectionRectangle(line, s.Length));
                }
                if (_highlighting == null
                    || !TryGetStringsToDraw(s, line, _highlighting.Highlightings.Where(x => x.IsOnLine(line)).ToList(), out var stringsToDraw))
                {
                    e.Graphics.DrawString(s, Font, Brushes.Black, new PointF(GetXCoordinateFromColumnIndex(0), GetYCoordinateFromLineIndex(line)));
                }
                else
                {
                    foreach ((string text, int characterOffset, Color colour) in stringsToDraw)
                    {
                        using (Brush brush = new SolidBrush(colour))
                        {
                            e.Graphics.DrawString(text, Font, brush, new PointF(GetXCoordinateFromColumnIndex(characterOffset), GetYCoordinateFromLineIndex(line)));
                        }
                    }
                }

                
                line++;
            }

            if (_highlighting != null)
            {
                foreach ((SourceCodePosition start, SourceCodePosition end, string _) in _highlighting.Errors)
                {
                    int startColumn = start.ColumnNumber;
                    for (int errorLine = start.LineNumber; errorLine <= end.LineNumber; errorLine++)
                    {
                        int endColumn = errorLine == end.LineNumber ? end.ColumnNumber : _sourceCode.Lines.ElementAt(errorLine).Length;
                        int y = errorLine * LINE_WIDTH + LINE_WIDTH - verticalScrollPositionPX;
                        int thisEndColumn = endColumn;
                        if (startColumn == endColumn)
                        {
                            thisEndColumn++;
                        }
                        int startX = GetXCoordinateFromColumnIndex(startColumn);
                        int endX = GetXCoordinateFromColumnIndex(thisEndColumn);
                        DrawSquigglyLine(e.Graphics, Pens.Red, startX, endX, y);
                        startColumn = 0;
                    }
                }
            }

            if (Focused)
            {
                ISelectionPosition position = _sourceCode.SelectionEnd;
                e.Graphics.DrawLine(Pens.Black,
                    new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber)),
                    new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber) + LINE_WIDTH));
            }
        }

        private void UpdateLineAndCharacterLabel()
        {
            lineLabel.Text = $"Ln: {_sourceCode.SelectionEnd.LineNumber} Ch: {_sourceCode.SelectionEnd.ColumnNumber}";
        }

        private void DrawSquigglyLine(Graphics g, Pen pen, int startX, int endX, int y)
        {
            List<Point> points = new List<Point>();
            int ySign = 1;
            for (int x = startX; x < endX; x += 4)
            {
                points.Add(new Point(x, y + 2 * ySign));
                ySign = -ySign;
            }
            if (points.Last().X != endX)
            {
                points.Add(new Point(endX, y));
            }
            g.DrawLines(pen, points.ToArray());
        }

        private bool TryGetStringsToDraw(string originalLine, int lineIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, out List<(string text, int characterOffset, Color colour)> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<(string text, int characterOffset, Color colour)>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    string before = originalLine.Substring(start, highlighting.Start.ColumnNumber - start);
                    stringsToDraw.Add((before, characterCount, Color.Black));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(0, highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((before, characterCount, Color.Black));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    string highlightedText = originalLine.Substring(0, highlighting.End.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add((originalLine, 0, highlighting.Colour));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add((originalLine.Substring(start), characterCount, Color.Black));
            }
            return true;
        }

        private Rectangle GetLineSelectionRectangle(int lineNumber, int lineCharacterLength)
        {
            var start = _sourceCode.SelectionStart;
            var end = _sourceCode.SelectionEnd;
            if (start.CompareTo(end) > 0)
            {
                (start, end) = (end, start);
            }

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
            return Rectangle.FromLTRB(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(startCharacterIndex),
                                      y,
                                      CURSOR_OFFSET + GetXCoordinateFromColumnIndex(endCharacterIndex),
                                      y + LINE_WIDTH);
        }

        private int GetXCoordinateFromColumnIndex(int columnIndex)
        {
            return columnIndex * _characterWidth - horizontalScrollPositionPX;
        }

        private int GetYCoordinateFromLineIndex(int lineIndex)
        {
            return lineIndex * LINE_WIDTH - verticalScrollPositionPX;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
            else if (e.Button == MouseButtons.Left)
            {
                (dragLineStart, dragColumnStart) = GetPositionFromMousePoint(e.Location);
                _sourceCode.SetActivePosition((int)dragLineStart, (int)dragColumnStart);
            }
            Refresh();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragLineStart != null
                && dragColumnStart != null
                && e.Button == MouseButtons.Left)
            {
                (int currentLine, int currentColumn) = GetPositionFromMousePoint(e.Location);
                _sourceCode.SelectRange((int)dragLineStart, (int)dragColumnStart, currentLine, currentColumn);
                Refresh();
            }
            else if (_highlighting != null)
            {
                (int currentLine, int currentColumn) = GetPositionFromMousePoint(e.Location);
                bool hoveringOverError = false;
                foreach ((SourceCodePosition start, SourceCodePosition end, string message) in _highlighting.Errors)
                {
                    int startColumn = start.ColumnNumber;
                    for (int line = start.LineNumber; line <= end.LineNumber; line++)
                    {
                        int endColumn = line == end.LineNumber ? end.ColumnNumber : _sourceCode.Lines.ElementAt(line).Length;
                        if (line == currentLine
                        && currentColumn >= startColumn
                        && currentColumn <= endColumn)
                        {
                            if (toolTip1.GetToolTip(panel1) != message)
                            {
                                toolTip1.SetToolTip(panel1, message);
                            }
                            hoveringOverError = true;
                            break;
                        }
                        startColumn = 0;
                    }
                }
                if (!hoveringOverError)
                {
                    toolTip1.SetToolTip(panel1, string.Empty);
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragLineStart = null;
            dragColumnStart = null;
        }

        private (int line, int column) GetPositionFromMousePoint(Point point)
        {
            return (Math.Max(0, (point.Y + verticalScrollPositionPX) / LINE_WIDTH),
                Math.Max(0, (point.X + horizontalScrollPositionPX) / _characterWidth));
        }

        private void CodeEditorBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // needed so that the KeyDown event picks up the arrowkeys
            if (e.KeyData.HasFlag(Keys.Right)
                || e.KeyData.HasFlag(Keys.Left)
                || e.KeyData.HasFlag(Keys.Up)
                || e.KeyData.HasFlag(Keys.Down)
                || e.KeyData.HasFlag(Keys.Tab))
            {
                e.IsInputKey = true;
            }
        }

        private void HandleCtrlShortcut(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    _sourceCode.SelectAll();
                    break;
                case Keys.X:
                    string selectedTextForCut = _sourceCode.GetSelectedText();
                    _sourceCode.RemoveSelectedRange();
                    if (!string.IsNullOrEmpty(selectedTextForCut))
                    {
                        Clipboard.SetText(selectedTextForCut);
                    }
                    break;
                case Keys.C:
                    string selectedTextForCopy = _sourceCode.GetSelectedText();
                    if (!string.IsNullOrEmpty(selectedTextForCopy))
                    {
                        Clipboard.SetText(selectedTextForCopy);
                    }
                    break;
                case Keys.V:
                    _sourceCode.InsertStringAtActivePosition(Clipboard.GetText());
                    UpdateSyntaxHighlighting();
                    break;

                case Keys.Left:
                    _sourceCode.ShiftActivePositionOneWordToTheLeft(e.Shift);
                    break;
                case Keys.Right:
                    _sourceCode.ShiftActivePositionOneWordToTheRight(e.Shift);
                    break;

                case Keys.Back:
                    _sourceCode.RemoveWordBeforeActivePosition();
                    break;
                case Keys.Delete:
                    _sourceCode.RemoveWordAfterActivePosition();
                    break;
            }
        }

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle all "normal" characters here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _specialCharacterHandler);
                UpdateSyntaxHighlighting();
                Refresh();
            }
        }

        private void CodeEditorBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // handle control keys here
            if (e.Control)
            {
                HandleCtrlShortcut(e);
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Back:
                        _sourceCode.RemoveCharacterBeforeActivePosition();
                        UpdateSyntaxHighlighting();
                        break;
                    case Keys.Delete:
                        _sourceCode.RemoveCharacterAfterActivePosition();
                        UpdateSyntaxHighlighting();
                        break;

                    case Keys.Left:
                        _sourceCode.ShiftActivePositionToTheLeft(e.Shift);
                        break;
                    case Keys.Right:
                        _sourceCode.ShiftActivePositionToTheRight(e.Shift);
                        break;
                    case Keys.Up:
                        _sourceCode.ShiftActivePositionUpOneLine(e.Shift);
                        break;
                    case Keys.Down:
                        _sourceCode.ShiftActivePositionDownOneLine(e.Shift);
                        break;
                    case Keys.End:
                        _sourceCode.ShiftActivePositionToEndOfLine(e.Shift);
                        break;
                    case Keys.Home:
                        _sourceCode.ShiftActivePositionToStartOfLine(e.Shift);
                        break;

                    case Keys.Enter:
                        _sourceCode.InsertLineBreakAtActivePosition(_specialCharacterHandler);
                        UpdateSyntaxHighlighting();
                        break;
                    case Keys.Tab:
                        _sourceCode.InsertStringAtActivePosition(SourceCode.TAB_REPLACEMENT);
                        UpdateSyntaxHighlighting();
                        break;
                }
            }
            Refresh();
        }
    }
}